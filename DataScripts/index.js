const fs = require('fs');
const path = require('path');
const sequences = require('./initialconditions.js');
const EPSILON = 2.220446049250313e-16;

//#region helpers
function minMag(a, b) {
  return (a > 0 ? Math.min : Math.max)(a, b);
}

function maxMag(a, b) {
  return (a > 0 ? Math.max : Math.min)(a, b);
}

function nextPow2(v) {
 v += v === 0
    --v
    v |= v >>> 1
    v |= v >>> 2
    v |= v >>> 4
    v |= v >>> 8
    v |= v >>> 16
    return v + 1 
}
//#endregion

let initialConditions;

//ODE45
var scratch = new Float64Array(1024);
var k1tmp, k2tmp, k3tmp, k4tmp, k5tmp, k6tmp, w;

function ode45(outputState, inputState, f, options) {
  if (typeof f !== 'function') {
    options = f;
    f = inputState;
    inputState = outputState;
  }
  var i, tmp, k1, k2, k3, k4, k5, k6;
  options = options || {};
  var out = outputState || {};

  var tolerance = options.tolerance === undefined ? 1e-8 : +options.tolerance;
  let tolerance2 = tolerance * tolerance;
  var maxIncreaseFactor =
    options.maxIncreaseFactor === undefined ? 10 : options.maxIncreaseFactor;
  var maxDecreaseFactor =
    options.maxDecreaseFactor === undefined ? 10 : options.maxDecreaseFactor;
  var tLimit =
    options.tLimit === undefined
      ? inputState.dt > 0.0
        ? Infinity
        : -Infinity
      : options.tLimit;

  var safetyFactor = 0.9;
  var y = inputState.y;
  var n = y.length;
  var dt = inputState.dt === undefined ? 1.0 : +inputState.dt;
  var t = inputState.t === undefined ? 0.0 : +inputState.t;
  if (out.y === undefined) {
    out.y = new Float64Array(n);
  }
  var yOut = out.y;
  var inPlace = yOut === y;

  if (n * 7 > scratch.length) {
    scratch = new Float64Array(nextPow2(n * 7));
  }
  if (!w || w.length !== n) {
    w = scratch.subarray(0, n);
    k1tmp = scratch.subarray(1 * n, 2 * n);
    k2tmp = scratch.subarray(2 * n, 3 * n);
    k3tmp = scratch.subarray(3 * n, 4 * n);
    k4tmp = scratch.subarray(4 * n, 5 * n);
    k5tmp = scratch.subarray(5 * n, 6 * n);
    k6tmp = scratch.subarray(6 * n, 7 * n);
  }

  var w = inPlace ? w : out;

  tmp = f(k1tmp, y, t);
  var returnsOutput = tmp !== undefined && tmp.length >= n;
  k1 = returnsOutput ? tmp : k1tmp;

  var limitReached = false;
  var trialStep = 0;
  var thisDt = dt;
  while (true && trialStep++ < 1000) {
    thisDt = minMag(thisDt, tLimit - t);

    for (i = 0; i < n; i++) {
      w[i] = y[i] + thisDt * (0.2 * k1[i]);
    }

    tmp = f(k2tmp, w, t + dt * 0.2);
    k2 = returnsOutput ? tmp : k2tmp;

    for (i = 0; i < n; i++) {
      w[i] = y[i] + thisDt * (0.075 * k1[i] + 0.225 * k2[i]);
    }
    tmp = f(k3tmp, w, t + thisDt * 0.3);
    k3 = returnsOutput ? tmp : k3tmp;

    for (i = 0; i < n; i++) {
      w[i] = y[i] + thisDt * (0.3 * k1[i] + -0.9 * k2[i] + 1.2 * k3[i]);
    }

    tmp = f(k4tmp, w, t + thisDt * 0.6);
    k4 = returnsOutput ? tmp : k4tmp;

    for (i = 0; i < n; i++) {
      w[i] =
        y[i] +
        thisDt *
          (-0.203703703703703703 * k1[i] +
            2.5 * k2[i] +
            -2.592592592592592592 * k3[i] +
            1.296296296296296296 * k4[i]);
    }

    tmp = f(k5tmp, w, t + thisDt);
    var k5 = returnsOutput ? tmp : k5tmp;

    for (i = 0; i < n; i++) {
      w[i] =
        y[i] +
        thisDt *
          (0.029495804398148148 * k1[i] +
            0.341796875 * k2[i] +
            0.041594328703703703 * k3[i] +
            0.400345413773148148 * k4[i] +
            0.061767578125 * k5[i]);
    }

    tmp = f(k6tmp, w, t + thisDt * 0.875);
    var k6 = returnsOutput ? tmp : k6tmp;

    var error2 = 0;
    for (var i = 0; i < n; i++) {
      let d =
        thisDt *
        (0.004293774801587301 * k1[i] +
          -0.018668586093857832 * k3[i] +
          0.034155026830808080 * k4[i] +
          0.019321986607142857 * k5[i] +
          -0.039102202145680406 * k6[i]);
      error2 += d * d;
    }

    if (error2 < tolerance2 || thisDt === 0.0) {
      break;
    }

    var nextDt = safetyFactor * thisDt * Math.pow(tolerance2 / error2, 0.1);
    thisDt = maxMag(thisDt / maxDecreaseFactor, nextDt);
  }

  for (var i = 0; i < n; i++) {
    y[i] +=
      thisDt *
      (0.097883597883597883 * k1[i] +
        0.402576489533011272 * k3[i] +
        0.210437710437710437 * k4[i] +
        0.289102202145680406 * k6[i]);
  }
  var previousDt = thisDt;
  out.t += thisDt;

  // Update dt for the next step (grow a bit if possible)
  nextDt = safetyFactor * thisDt * Math.pow(tolerance2 / error2, 0.125);
  out.dt = maxMag(
    thisDt / maxDecreaseFactor,
    minMag(thisDt * maxIncreaseFactor, nextDt)
  );
  out.dtPrevious = thisDt;
  out.limitReached =
    isFinite(tLimit) &&
    Math.abs((out.t - options.tLimit) / previousDt) < EPSILON;

  return out;
}

//attractions
function planarThreeBodyDerivative(yp, y, t) {
  let dx, dy, r3, fx, fy
  let m0 = initialConditions.M[0]
  let m1 = initialConditions.M[1]
  let m2 = initialConditions.M[2]

  // d(position)/dt = velocity
  yp[0] = y[2]; yp[1] = y[3];
  yp[4] = y[6]; yp[5] = y[7];
  yp[8] = y[10]; yp[9] = y[11];

  // Pairwise gravitational attractions
  dx = y[4] - y[0]
  dy = y[5] - y[1]
  r3 = Math.pow(dx * dx + dy * dy, 1.5)
  dx /= r3, dy /= r3
  yp[2] = dx * m1
  yp[3] = dy * m1
  yp[6] = -dx * m0
  yp[7] = -dy * m0

  dx = y[8] - y[0]
  dy = y[9] - y[1]
  r3 = Math.pow(dx * dx + dy * dy, 1.5)
  dx /= r3, dy /= r3
  yp[2] += dx * m2
  yp[3] += dy * m2
  yp[10] = -dx * m0
  yp[11] = -dy * m0

  dx = y[8] - y[4]
  dy = y[9] - y[5]
  r3 = Math.pow(dx * dx + dy * dy, 1.5)
  dx /= r3, dy /= r3
  yp[6] += dx * m2
  yp[7] += dy * m2
  yp[10] -= dx * m1
  yp[11] -= dy * m1

  return yp;
}

//trajectory
function trajectory() {
  let config = {
    tolerance: initialConditions.tolerance || 1e-9,
    tLimit: initialConditions.period
  };

  let state = { t: 0, y: initialConditions.y0.slice() };
  let result = { position: [], t: [] };
  
  function storeStep(t, y) {
    result.position.push(y[0], y[1], t, y[4], y[5], t, y[8], y[9], t);
    result.t.push(t);
  }

  storeStep(state.t, state.y);

  let step = 0;
  while(step++ < 1e6 && !state.limitReached) {
    ode45(state, planarThreeBodyDerivative, config);
    storeStep(state.t, state.y);
  }

  return result;
}

if (require.main === module) {
  sequences.forEach(({sequence, orbits}) => {
    orbits.forEach(orbit => {
      initialConditions = {
        M: orbit.M || [1.0, 1.0, 1.0],
        tolerance: 1e-9,
        period: orbit.T,
        y0: [
          orbit.x[0][0], orbit.x[0][1], orbit.v[0][0], orbit.v[0][1],
          orbit.x[1][0], orbit.x[1][1], orbit.v[1][0], orbit.v[1][1],
          orbit.x[2][0], orbit.x[2][1], orbit.v[2][0], orbit.v[2][1]
        ]
      };

    const traj = trajectory();

    //#region csv
    const header = ['t', 'x0','y0', 'x1','y1', 'x2','y2'].join(';') + '\n';
    const body = traj.t.map((t,i) => {
      const base = i * 9;
      const x0 = traj.position[base + 0];
      const y0 = traj.position[base + 1];
      const x1 = traj.position[base + 3];
      const y1 = traj.position[base + 4];
      const x2 = traj.position[base + 6];
      const y2 = traj.position[base + 7];
      return [t, x0, y0, x1, y1, x2, y2].join(';');
    }).join('\n');
    const safe = str => str.replace(/\s+/g,'_').replace(/[^\w_-]/g,'');
    const csvFileName = `${safe(sequence)}-${safe(orbit.name)}.csv`;
    const csvOutPath  = path.join(__dirname, 'csvoutput', csvFileName);
    fs.writeFileSync(csvOutPath, header + body);
    //#endregion

    //#region html
    const html = `<!DOCTYPE html>
    <html><head><meta charset="utf-8" /><script src="https://cdn.plot.ly/plotly-latest.min.js"></script></head>
    <body>
      <div id="plot" style="width:100%;height:100vh;"></div>
      <script>
        const result = ${JSON.stringify(traj)};
        const x0 = [], y0 = [], x1 = [], y1 = [], x2 = [], y2 = [];
        for (let i = 0; i < result.position.length; i += 3) {
          x0.push(result.position[i+0]);  y0.push(result.position[i+1]);
          x1.push(result.position[i+3]);  y1.push(result.position[i+4]);
          x2.push(result.position[i+6]);  y2.push(result.position[i+7]);
        }
        const N = x0.length;
    
        const traces = [
          { x: x0, y: y0, mode: 'markers', name: 'Body 0' },
          { x: x1, y: y1, mode: 'markers', name: 'Body 1' },
          { x: x2, y: y2, mode: 'markers', name: 'Body 2' }
        ];
    
        Plotly.newPlot('plot', traces, {
          title: 'Planar Three-Body Orbits (Labeled Points)',
          xaxis: { title: 'x' },
          yaxis: { title: 'y', scaleanchor: 'x', scaleratio: 1 }
        },
        {
          scrollZoom: true
        });
      </script>
    </body></html>`;
    const htmlFileName = `${safe(sequence)}-${safe(orbit.name)}.html`;
    const htmlOutPath = path.join(__dirname, 'htmloutput', htmlFileName);
    fs.writeFileSync(htmlOutPath, html);
    //#endregion
    });
  });
}
