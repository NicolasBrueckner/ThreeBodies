const Broucke = require("./sequencemodules/Broucke");
const Henon = require("./sequencemodules/Henon");
const I_butterfly = require("./sequencemodules/_I_butterfly");
const II_dragonfly = require("./sequencemodules/_II_dragonfly");
const III_yin_yang = require("./sequencemodules/_III_yin_yang");
const IVa_moth = require("./sequencemodules/_IVa_moth");
const IVb_butterfly = require("./sequencemodules/_IVb_butterfly");
const IVc_moth = require("./sequencemodules/_IVc_moth");
const V_figure_8 = require("./sequencemodules/_V_figure_8");
const VI_yarn = require("./sequencemodules/_VI_yarn");
const VIIa_moth = require("./sequencemodules/_VIIa_moth");
const VIIb_moth = require("./sequencemodules/_VIIb_moth");
const VIII_other = require("./sequencemodules/_VIII_other");
const Sheen = require("./sequencemodules/_Sheen");
const IA_ic_equalMass = require("./sequencemodules/_IA_ic_equalMass");
const IB_ic_equalMass = require("./sequencemodules/_IB_ic_equalMass");
const IC_ic_equalMass = require("./sequencemodules/_IC_ic_equalMass");
const IA_ic = require("./sequencemodules/_IA_ic");
const IB_ic = require("./sequencemodules/_IB_ic");
const IC_ic = require("./sequencemodules/_IC_ic");
const IIA_ic = require("./sequencemodules/_IIA_ic");
const IIB_ic = require("./sequencemodules/_IIB_ic");
const IIC_ic = require("./sequencemodules/_IIC_ic");
const IID_ic = require("./sequencemodules/_IID_ic");
const Freefall = require("./sequencemodules/_Freefall");
const figureEight = require("./sequencemodules/_figureEight");
const hristov2_src = require("./sequencemodules/_hristov2_src");

function Hristov2()
{
    const orbits = {};
    for (let [name, ic] of Object.entries(hristov2_src)) {
        ic.x = [[-1, 0], [1, 0], [0, 0]];
        ic.ref = "hristov1";
        ic.L = 0;
        ic.url = "https://github.com/rgoranova/3body";
        ic.year = "2021";
        ic.v[1] = ic.v[0].slice();
        ic.v[2] = ic.v[0].slice().map((x) => -x * 2);

        orbits[`${name} ${ic.old ? "(old)" : ""}`] = ic;
    }
    return orbits;
}

function FigureEight()
{
    const orbits = {};
    for (let [name, ic] of Object.entries(figureEight)) {
        ic.x = [[-1, 0], [1, 0], [0, 0]];
        ic.ref = "fe";
        ic.L = 0;
        ic.url = "http://db2.fmi.uni-sofia.bg/3body/";
        ic.year = "2022";
        ic.v[1] = ic.v[0].slice();
        ic.v[2] = ic.v[0].slice().map((x) => -x * 2);

        orbits[`${name} ${ic.old ? "(old)" : ""}`] = ic;
    }
    return orbits;
}

module.exports = {
"Šuvakov": {
    "I - Butterfly I": I_butterfly,
    "II - Dragonfly": II_dragonfly,
    "III - Yin Yang": III_yin_yang,
    "IVa - Moth I": IVa_moth,
    "IVb - Butterfly III": IVb_butterfly,
    "IVc - Moth III": IVc_moth,
    "V - Figure 8": V_figure_8,
    "VI - Yarn": VI_yarn,
    "VIIa - Moth": VIIa_moth,
    "VIIb - Moth": VIIb_moth,
    "VIII - Other": VIII_other,
    "Broucke": Broucke,
    "Sheen": Sheen,
    "Henon": Henon
  },
  "Li & Liao Equal Mass": {
    "I.A i.c. equal mass": IA_ic_equalMass,
    "I.B i.c. equal mass": IB_ic_equalMass,
    "I.C i.c. equal mass": IC_ic_equalMass
  },
  "Li & Liao Unequal Mass": {
    "I.A i.c.": IA_ic,
    "I.B i.c.": IB_ic,
    "I.C i.c.": IC_ic,
    "II.A i.c.": IIA_ic,
    "II.B i.c.": IIB_ic,
    "II.C i.c.": IIC_ic,
    "II.D i.c.": IID_ic
  },
  "Li & Liao Free-fall": {
    "Free Fall": Freefall
  },
  "Hristov et al.": {
    "Set One": Hristov2(),
    "Figure Eight": FigureEight()
  }
}
