const fs = require('fs');
const path = require('path');
const conditions = require('./initialconditions.js');
const EPSILON = 2.220446049250313e-16;
const safe = s => s.replace(/\s+/g,'_').replace(/[^\w_-]/g,'');

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
  let m0 = initialConditions.m[0]
  let m1 = initialConditions.m[1]
  let m2 = initialConditions.m[2]

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
    tolerance: initialConditions.tolerance,
    tLimit: initialConditions.tEnd
  };

  let state = { t: 0, y: initialConditions.y0.slice() };
  let result = { position: [], t: [] };
  
  function storeStep(t, y) {
    result.position.push(y[0], y[1], y[4], y[5], y[8], y[9]);
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
  //GenerateBinaryPerSequence();
  
  const seqFile = path.join(__dirname, 'binary_sequence', 'Broucke.bytes');
  const allOrbits = readSequenceFile(seqFile);
  plotOrbit(allOrbits, 0);
  
  /*
  console.log(`Found ${allOrbits.length} orbits in IC_ic.bytes`);
  allOrbits.forEach((orb, i) => {
    console.log(
      `Orbit #${i}: name="${orb.orbitName}", M=${orb.M}, T=${orb.T}`
    );
  });*/
}


//per orbit output structure:
//[M: float(4)][t: float16(M*4)][pos: float16(6*4*M)]                                 - 4 + 28M
//[name: uint, string(4+uint)][year: uint, string(4+uint)][g: uint, string(4+uint)]
//[T: float(4)][E: float(4)][L: float(4)][m1: float(4)][m2: float(4)][m3: float(4)]   - 36 + 3uint
function GenerateBinaryPerSequence() {
  const outDir = path.join(__dirname, 'binary_sequence');
  fs.mkdirSync(outDir, { recursive: true });

  let biggestOrbit = {number: 0, orbitName:'', sequenceName:''};

  for (const [groupName, sequences] of Object.entries(conditions)) {
    for (const [sequenceName, orbits] of Object.entries(sequences)) {
      console.log(`Writing binary for ${groupName} / ${sequenceName}…`);

      const filename = `${safe(sequenceName)}.bytes`;
      const ws = fs.createWriteStream(path.join(outDir, filename));

      for (const [orbitName, ic] of Object.entries(orbits)) {
        const parts = [];

        //update initial conditions
        initialConditions = {
          m:         ic.m         || [1,1,1],
          tolerance: ic.tolerance || 1e-9,
          tEnd:      ic.T,
          y0: [
            ic.x[0][0], ic.x[0][1], ic.v[0][0], ic.v[0][1],
            ic.x[1][0], ic.x[1][1], ic.v[1][0], ic.v[1][1],
            ic.x[2][0], ic.x[2][1], ic.v[2][0], ic.v[2][1],
          ]
        }
        //calculate all positions
        const fullTraj = trajectory();
        const {t: Td, position: Pd} = decimateTrajectories(fullTraj, 0.2);
        const t32 = Float32Array.from(Td);
        const p32 = Float32Array.from(Pd);
        const M = t32.length;
        let offs = 0;

        if(orbitName==='Broucke A 1') {
          debugPlotRawPoints(p32, M);
        }

        //write M into buffer (length of t and pos values)
        const Mbuf = Buffer.allocUnsafe(4);
        Mbuf.writeInt32LE(M, offs);
        parts.push(Mbuf);

        const tbuf = Buffer.allocUnsafe(4*M);
        offs = 0;
        for (let i = 0; i < M; i++, offs += 4) {
          tbuf.writeFloatLE(t32[i], offs);
        }
        parts.push(tbuf);
        
        //write t and p arrays into buffer based on step
        const pbuf = Buffer.allocUnsafe(4*6*M);
        offs = 0;
        for (let i = 0; i < M; i++) {
          const b = i*6;
          pbuf.writeFloatLE(p32[b+0], offs); offs += 4;
          pbuf.writeFloatLE(p32[b+1], offs); offs += 4;
          pbuf.writeFloatLE(p32[b+2], offs); offs += 4;
          pbuf.writeFloatLE(p32[b+3], offs); offs += 4;
          pbuf.writeFloatLE(p32[b+4], offs); offs += 4;
          pbuf.writeFloatLE(p32[b+5], offs); offs += 4;
        }
        parts.push(pbuf);

        const nameStr = Buffer.from(orbitName, 'utf8');
        const nameLen = Buffer.allocUnsafe(4);
        nameLen.writeInt32LE(nameStr.length, 0);
        parts.push(nameLen, nameStr);

        const yearStr = Buffer.from(ic.year ?? '', 'utf8');
        const yearLen = Buffer.allocUnsafe(4);
        yearLen.writeInt32LE(yearStr.length, 0);
        parts.push(yearLen, yearStr);

        const gStr = Buffer.from(ic.G ?? '', 'utf8');
        const gLen = Buffer.allocUnsafe(4);
        gLen.writeInt32LE(gStr.length, 0);
        parts.push(gLen, gStr);

        //write T, E, L and mass into buffer
        offs=0;
        const metaBuf = Buffer.allocUnsafe(6*4);
        metaBuf.writeFloatLE(ic.T ?? -1, offs); offs += 4;
        metaBuf.writeFloatLE(ic.E ?? -1, offs); offs += 4;
        metaBuf.writeFloatLE(ic.L ?? -1, offs); offs += 4;
        metaBuf.writeFloatLE(ic.m?.[0] ?? 1, offs); offs += 4;
        metaBuf.writeFloatLE(ic.m?.[1] ?? 1, offs); offs += 4;
        metaBuf.writeFloatLE(ic.m?.[2] ?? 1, offs);
        parts.push(metaBuf);
          
        if(M>biggestOrbit.number)
        {
          biggestOrbit.number=M;
          biggestOrbit.orbitName=orbitName;
          biggestOrbit.sequenceName=sequenceName;
        }
        //concat parts and write to file
        const buf = Buffer.concat(parts);
        ws.write(buf);
      }
      
      ws.end(() => {
        console.log(` → Finished ${filename}`);
      });
    }
    console.log(`biggest orbit: ${biggestOrbit.number}, ${biggestOrbit.orbitName}, ${biggestOrbit.sequenceName}, group: ${groupName}`);
  }
}

function decimateTrajectories(traj, p) {
  const N = traj.t.length;
  if (p >= 1 || N === 0)
    return { t: traj.t.slice(), position: traj.position.slice() };
  
  const M = Math.max(1, Math.floor(N * p));
  const step = N / M; 

  const t2   = new Array(M);
  const pos2 = new Array(M * 6);

  for (let j = 0; j < M; j++) {
    const i = Math.floor(j * step);
    t2[j] = traj.t[i];

    const srcBase = i * 6;
    const dstBase = j * 6;
    for (let k = 0; k < 6; k++)
      pos2[dstBase + k] = traj.position[srcBase + k];
  }

  return { t: t2, position: pos2 };
}

function readSequenceFile(filePath) {
  const buf = fs.readFileSync(filePath);
  const orbits = [];
  let off = 0;

  while (off < buf.length) {
    // 1) number of timesteps M
    const M = buf.readInt32LE(off);
    off += 4;

    // 2) M float32’s for t
    const t = new Float32Array(M);
    for (let i = 0; i < M; i++, off += 4) {
      t[i] = buf.readFloatLE(off);
    }

    // 3) 6 Floats (x0, y0, x1, y1, x2, y2) for positions
    const p = new Float32Array(6 * M);
    for (let i = 0; i < 6 * M; i++, off += 4) {
      p[i] = buf.readFloatLE(off);
    }

    // 4) orbitName string
    const nameLen = buf.readInt32LE(off);
    off += 4;
    const orbitName = buf.toString('utf8', off, off + nameLen);
    off += nameLen;

    // 5) year string
    const yearLen = buf.readInt32LE(off);
    off += 4;
    const year = buf.toString('utf8', off, off + yearLen);
    off += yearLen;

    // 6) G string
    const gLen = buf.readInt32LE(off);
    off += 4;
    const G = buf.toString('utf8', off, off + gLen);
    off += gLen;

    // 7) metadata floats: T, E, L, m0, m1, m2
    const T = buf.readFloatLE(off); off += 4;
    const E = buf.readFloatLE(off); off += 4;
    const L = buf.readFloatLE(off); off += 4;
    const m0 = buf.readFloatLE(off); off += 4;
    const m1 = buf.readFloatLE(off); off += 4;
    const m2 = buf.readFloatLE(off); off += 4;

    orbits.push({ M, t, p, orbitName, year, G, T, E, L, m: [m0, m1, m2] });
  }

  return orbits;
}

function plotOrbit(orbits, i, outputPath = 'orbit_plot.html') {
  const orbit = orbits[i];
  const M = orbit.M;
  const p = orbit.p;

  const x0 = [], y0 = [];
  const x1 = [], y1 = [];
  const x2 = [], y2 = [];

  for (let j = 0; j < M; j++) {
    x0.push(p[j * 6 + 0]);
    y0.push(p[j * 6 + 1]);

    x1.push(p[j * 6 + 2]);
    y1.push(p[j * 6 + 3]);

    x2.push(p[j * 6 + 4]);
    y2.push(p[j * 6 + 5]);
  }

  const html = `
    <!DOCTYPE html>
    <html>
      <head>
        <meta charset="utf-8" />
        <script src="https://cdn.plot.ly/plotly-latest.min.js"></script>
      </head>
      <body>
        <style>
          html, body {
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
            overflow: hidden;
          }
          #plot {
            width: 100%;
            height: 100%;
          }
        </style>
        <div id="plot"></div>
        <script>
          Plotly.newPlot('plot', [
            {
              x: ${JSON.stringify(x0)},
              y: ${JSON.stringify(y0)},
              mode: 'markers',
              name: 'Body 0'
            },
            {
              x: ${JSON.stringify(x1)},
              y: ${JSON.stringify(y1)},
              mode: 'markers',
              name: 'Body 1'
            },
            {
              x: ${JSON.stringify(x2)},
              y: ${JSON.stringify(y2)},
              mode: 'markers',
              name: 'Body 2'
            }
          ], {
            title: 'Orbit ${i}: ${orbit.orbitName}',
            xaxis: { title: 'X Position', range: [-5, 5] },
            yaxis: { title: 'Y Position', range: [-5, 5] },
            showlegend: true
          }, {responsive: true});
        </script>
      </body>
    </html>`;

  fs.writeFileSync(outputPath, html);
  console.log(`Plot saved to ${outputPath}`);
}

function debugPlotRawPoints(p32, M, name = 'debug_plot.html') {
  const x0 = [], y0 = [];
  const x1 = [], y1 = [];
  const x2 = [], y2 = [];

  for (let j = 0; j < M; j++) {
    const i = j * 6;
    x0.push(p32[i + 0]);
    y0.push(p32[i + 1]);
    x1.push(p32[i + 2]);
    y1.push(p32[i + 3]);
    x2.push(p32[i + 4]);
    y2.push(p32[i + 5]);
  }

  const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8" />
      <script src="https://cdn.plot.ly/plotly-latest.min.js"></script>
      <style>
        html, body { margin: 0; padding: 0; width: 100%; height: 100%; }
        #plot { width: 100%; height: 100%; }
      </style>
    </head>
    <body>
      <div id="plot"></div>
      <script>
        Plotly.newPlot('plot', [
          { x: ${JSON.stringify(x0)}, y: ${JSON.stringify(y0)}, mode: 'markers', name: 'Body 0' },
          { x: ${JSON.stringify(x1)}, y: ${JSON.stringify(y1)}, mode: 'markers', name: 'Body 1' },
          { x: ${JSON.stringify(x2)}, y: ${JSON.stringify(y2)}, mode: 'markers', name: 'Body 2' }
        ], {
          title: 'Raw Trajectory Plot',
          xaxis: { title: 'X' },
          yaxis: { title: 'Y' },
          showlegend: true
        }, {responsive: true});
      </script>
    </body>
    </html>
  `;

  fs.writeFileSync(name, html);
  console.log(`Raw plot written to ${name}`);
}
