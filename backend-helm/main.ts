import { Construct } from 'constructs';
import { App, Chart, ChartProps } from 'cdk8s';
import { Deployment } from 'cdk8s-plus-27';

function defineResources(chart: Chart) {
    const appDeployment = new Deployment(chart, 'app-deployment', {
      containers: [{ image: "ghcr.io/completeinformation/household-backend:0.1.0-alpha.1", ports: [{ number: 5000 }], securityContext: {
        ensureNonRoot: true
      } }],
      replicas: 1,
    });
    appDeployment.exposeViaService({
      ports: [{ port: 8080, targetPort: 5000 }]
    });
}

export class MyChart extends Chart {
  constructor(scope: Construct, id: string, props: ChartProps = { }) {
    super(scope, id, props);

    defineResources(this);
  }
}

const app = new App();
new MyChart(app, 'backend-helm');
app.synth();
