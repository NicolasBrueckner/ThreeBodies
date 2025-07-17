const fs = require('fs');
const figureEight = require('./sequencemodules/_figureEight.js');

function FigureEight()
{
    const orbits = {};
    for (let [name, ic] of Object.entries(figureEight)) {
        ic.x = [[-1, 0], [1, 0], [0, 0]];
        ic.L = 0;
        ic.year = "2022";
        ic.v[1] = ic.v[0].slice();
        ic.v[2] = ic.v[0].slice().map((x) => -x * 2);

        orbits[`${name.trim()}${ic.old ? " (old)" : ""}`] = ic;
    }
    return orbits;
}

const converted = FigureEight();

// Save to JSON file
fs.writeFileSync('figure8_converted.json', JSON.stringify(converted, null, 2));
console.log('Converted JSON saved to figure8_converted.json');