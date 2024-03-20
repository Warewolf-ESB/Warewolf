import {Model} from '@antv/x6';
import {DagreLayout} from '@antv/layout';
import {loadGraph} from './load-graph';
import {graphBindings} from './graph-bindings';

const dagreLayout = new DagreLayout({
    type: 'dagre',
    rankdir: 'LR',
    align: 'DL',
    ranksep: 35,
    nodesep: 15,
})

export async function autoLayout(graphId: string, data: string | Model.FromJSONData) {
    const {graph, interop} = graphBindings[graphId];
    const model = typeof data === 'string' ? JSON.parse(data) : data;
    const newModel = dagreLayout.layout(model);

    loadGraph(graphId, newModel);
    await interop.raiseGraphUpdated();
}