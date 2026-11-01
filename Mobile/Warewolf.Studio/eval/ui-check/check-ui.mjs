// Checks one organization UI for the bakeoff's `uis` task (ArtifactBakeOff, UiCheck.kt): compiles its TypeScript,
// starts it in the studio's real sandboxed frame and Worker runtime, fills every control, presses submit, answers the
// runWorkflow call with stand-in outputs, and reports what it sent and what it showed. No engine is involved: the
// outputs are markers (OUT_<name>) so the check can tell whether the UI displays them.
//
//   node check-ui.mjs <angularDir> <request.json>
//
// <angularDir> is the Web Studio checkout (~/angular): its esbuild, Playwright (with Chromium installed) and the
// built runtime in src/assets/organization-ui. <request.json> is {source, inputs: [{name, kind, fields}],
// outputs: [{name, kind, fields}]}. Prints one JSON line: the Result UiCheck.kt reads.
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';

const [angularDir, requestFile] = process.argv.slice(2);
const require = createRequire(path.join(path.resolve(angularDir), 'package.json'));
const esbuild = require('esbuild');
const { chromium } = require('playwright');
const request = JSON.parse(fs.readFileSync(requestFile, 'utf8'));

/** What the runtime lets a UI import (worker.ts MODULES). */
const ALLOWED = new Set(['@angular/core', '@angular/common', '@angular/forms', '@warewolf/ui']);
const KEY = 'main';

const result = {
  compiled: false, started: false, ran: false, runKey: null,
  inputsSent: [], inputsMissing: [], outputsShown: [], outputsMissing: [], errors: [],
};
const done = () => { console.log(JSON.stringify(result)); process.exit(0); };

const imports = [...request.source.matchAll(/(?:from\s+|import\s+|require\(\s*)['"]([^'"]+)['"]/g)].map(m => m[1]);
const refused = [...new Set(imports.filter(name => !ALLOWED.has(name)))];
if (refused.length) {
  result.errors.push(`imports a module the UI can't use: ${refused.join(', ')} (only ${[...ALLOWED].join(', ')})`);
  done();
}

let script;
try {
  // As scripts/build-organization-ui-runtime.mjs builds the runtime, plus the decorators a component is written with.
  const code = (await esbuild.transform(request.source, {
    loader: 'ts', format: 'cjs', target: 'es2016', // es2016 lowers async/await, which zone.js can't see, so change detection would miss a run's result.
    tsconfigRaw: { compilerOptions: { experimentalDecorators: true, useDefineForClassFields: false } },
  })).code;
  // The runtime calls a script with exports and require only (worker.ts load); esbuild's CommonJS sets module.exports.
  script = `var module = { exports: exports };\n${code}\nif (module.exports !== exports) Object.assign(exports, module.exports);`;
  result.compiled = true;
} catch (error) {
  result.errors.push(`does not compile: ${(error.errors ?? []).map(e => e.text).join('; ') || error.message}`);
  done();
}

/** A stand-in value per output, so the check can find it on the page. */
function standIn(output) {
  const marker = `OUT_${output.name}`;
  if (output.kind === 'recordset') return [Object.fromEntries((output.fields?.length ? output.fields : ['value']).map(f => [f, `${marker}_${f}`]))];
  if (output.kind === 'object') return { value: marker };
  return marker;
}
const outputs = Object.fromEntries(request.outputs.map(o => [o.name, standIn(o)]));

const { frameDocument } = await (async () => {
  const { outputFiles } = await esbuild.build({
    entryPoints: [path.join(angularDir, 'src/app/organization/organization-uis/organization-ui-frame.ts')],
    bundle: true, write: false, format: 'cjs', platform: 'node',
  });
  const module = { exports: {} };
  new Function('module', 'exports', outputFiles[0].text)(module, module.exports);
  return module.exports;
})();

const studio = http.createServer((req, res) => {
  if (req.url.startsWith('/assets/')) {
    const file = path.join(angularDir, 'src', req.url);
    if (fs.existsSync(file)) { res.setHeader('content-type', 'text/javascript'); return res.end(fs.readFileSync(file)); }
    res.statusCode = 404; return res.end();
  }
  res.setHeader('content-type', 'text/html');
  res.end('<!doctype html><title>studio</title><body></body>');
}).listen(0, '127.0.0.1');
await new Promise(resolve => studio.once('listening', resolve));
const origin = `http://127.0.0.1:${studio.address().port}`;

const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  await page.goto(origin + '/studio');
  // Plays the studio page as organization-ui-page does: the same frame, and an answer to each run request.
  await page.evaluate(({ doc, script, outputs }) => {
    window.log = [];
    const frame = document.createElement('iframe');
    frame.setAttribute('sandbox', 'allow-scripts');
    frame.setAttribute('referrerpolicy', 'no-referrer');
    frame.style.width = '1000px'; frame.style.height = '800px';
    window.addEventListener('message', e => {
      if (e.source !== frame.contentWindow) return;
      window.log.push(e.data);
      if (e.data.type === 'ready') frame.contentWindow.postMessage({ type: 'start', script }, '*');
      if (e.data.type === 'run') frame.contentWindow.postMessage({ type: 'result', id: e.data.id, outputs }, '*');
    });
    frame.srcdoc = doc;
    document.body.appendChild(frame);
  }, { doc: frameDocument(origin), script, outputs });
  const frame = page.frames()[1] ?? await page.waitForEvent('frameattached');
  const log = () => page.evaluate(() => window.log);
  const failures = async () => (await log()).filter(m => m.type === 'failed').map(m => m.message);

  await page.waitForFunction(() => window.log.some(m => m.type === 'started' || m.type === 'failed'), null, { timeout: 30_000 }).catch(() => {});
  result.started = (await log()).some(m => m.type === 'started');
  if (!result.started) {
    result.errors.push(`does not start: ${(await failures()).join('; ') || 'no answer from the runtime within 30s'}`);
  } else {
    await page.waitForTimeout(300);
    // Every control gets a value its type accepts; recordset inputs get one row through whatever controls they offer.
    const controls = await frame.$$('input, textarea, select');
    for (const [i, control] of controls.entries()) {
      const type = await control.evaluate(el => el.tagName === 'INPUT' ? (el.getAttribute('type') || 'text').toLowerCase() : el.tagName.toLowerCase());
      try {
        if (type === 'checkbox' || type === 'radio') await control.check({ timeout: 1000 });
        else if (type === 'select') {
          const count = await control.evaluate(el => el.options.length);
          if (count > 1) await control.selectOption({ index: 1 }, { timeout: 1000 });
        } else if (!['submit', 'button', 'hidden', 'file', 'image', 'reset', 'range', 'color'].includes(type)) {
          const value = { number: '42', date: '2024-01-15', 'datetime-local': '2024-01-15T10:30', time: '10:30', email: 'ada@example.com', url: 'https://example.com', tel: '0821234567', month: '2024-01' }[type] ?? `sample${i + 1}`;
          await control.fill(value, { timeout: 1000 });
        }
      } catch { /* a disabled or hidden control: leave it */ }
    }
    const submit = await frame.$('button[type=submit], input[type=submit]')
      ?? (await frame.$$('button')).at(0);
    if (!submit) result.errors.push('has no button to run the workflow');
    else await submit.click({ timeout: 2000 }).catch(error => result.errors.push(`its run button can't be pressed: ${error.message.split('\n')[0]}`));

    await page.waitForFunction(() => window.log.some(m => m.type === 'run'), null, { timeout: 5_000 }).catch(() => {});
    const run = (await log()).find(m => m.type === 'run');
    if (!run) {
      result.errors.push(`never runs the workflow: after filling every control and pressing ${submit ? 'its button' : 'nothing'}, it made no runWorkflow call${(await failures()).length ? ` (${(await failures()).join('; ')})` : ''}`);
    } else {
      result.ran = true;
      result.runKey = run.key;
      if (run.key !== KEY) result.errors.push(`runs runWorkflow('${run.key}', …), but the workflow's key is '${KEY}'`);
      const sent = run.inputs && typeof run.inputs === 'object' && !Array.isArray(run.inputs) ? run.inputs : {};
      for (const input of request.inputs) {
        const value = sent[input.name];
        const empty = value === undefined || value === null || value === '' || (Array.isArray(value) && value.length === 0);
        (empty ? result.inputsMissing : result.inputsSent).push(input.name);
      }
      const extra = Object.keys(sent).filter(name => !request.inputs.some(input => input.name === name));
      if (result.inputsMissing.length) result.errors.push(`sends no value for input(s) ${result.inputsMissing.join(', ')} after every control was filled`);
      if (extra.length) result.errors.push(`sends ${extra.join(', ')}, which the workflow does not take as inputs`);

      await page.waitForTimeout(1500);
      const text = await frame.evaluate(() => document.body.innerText);
      for (const output of request.outputs) (text.includes(`OUT_${output.name}`) ? result.outputsShown : result.outputsMissing).push(output.name);
      if (result.outputsMissing.length) result.errors.push(`never shows output(s) ${result.outputsMissing.join(', ')} from the workflow's reply`);
    }
    for (const message of await failures()) result.errors.push(`failed while running: ${message}`);
  }
} catch (error) {
  result.errors.push(`check error: ${error.message.split('\n')[0]}`);
} finally {
  await browser.close();
  studio.close();
}
done();
