const fs = require('fs');
const hristov2_src = require('./sequencemodules/_hristov2_src.js');

function Hristov2() {
    const orbits = {};
    for (let [name, ic] of Object.entries(hristov2_src)) {
        ic.x = [[-1, 0], [1, 0], [0, 0]];
        ic.L = 0;
        ic.year = "2021";
        ic.v[1] = ic.v[0].slice();
        ic.v[2] = ic.v[0].slice().map((x) => -x * 2);

        orbits[`${name.trim()}${ic.old ? " (old)" : ""}`] = ic;
    }
    return orbits;
}

const converted = Hristov2();

// Save to JSON file
fs.writeFileSync('hristov2_converted.json', JSON.stringify(converted, null, 2));
console.log('Converted JSON saved to hristov2_converted.json');