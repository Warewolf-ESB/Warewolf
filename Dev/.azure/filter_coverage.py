import xml.etree.ElementTree as ET
import re
import glob
import sys

keep = re.compile(r'^(Dev2\.|Warewolf\.)')
deny = re.compile(
    r'^(Warewolf\.Weave'
    r'|Elastic\.Clients\.Elasticsearch'
    r'|Microsoft\.Azure\.WebJobs\.'
    r'|WebJobs\.'
    r'|StackExchange\.Redis'
    r'|ServiceStack'
    r'|Microsoft\.Azure\.AppService'
    r'|FSharp\.Core'
    r'|DotNetWorker\.Grpc'
    r'|Microsoft\.Azure\.Functions\.Worker\.Grpc'
    r')'
)


def filter_xml(path):
    tree = ET.parse(path)
    root = tree.getroot()
    pkgs = root.find('packages')
    if pkgs is None:
        return
    removed = [p for p in list(pkgs) if not keep.match(p.get('name', '')) or deny.match(p.get('name', ''))]
    for p in removed:
        pkgs.remove(p)
    lc = lv = bc = bv = 0
    for line in root.iter('line'):
        lv += 1
        if int(line.get('hits', '0')) > 0:
            lc += 1
        if line.get('branch') == 'True':
            for cond in line.findall('conditions/condition'):
                bv += 1
                if cond.get('coverage', '0%') != '0%':
                    bc += 1
    root.set('lines-covered', str(lc))
    root.set('lines-valid', str(lv))
    root.set('branches-covered', str(bc))
    root.set('branches-valid', str(bv))
    root.set('line-rate', f'{lc/lv:.6f}' if lv else '0')
    root.set('branch-rate', f'{bc/bv:.6f}' if bv else '0')
    tree.write(path, xml_declaration=True, encoding='unicode')
    print(f'{path}: removed {len(removed)} packages, {lc}/{lv} lines, {bc}/{bv} branches')


merged_dir = sys.argv[1]
for f in glob.glob(f'{merged_dir}/*.cobertura.xml'):
    filter_xml(f)
