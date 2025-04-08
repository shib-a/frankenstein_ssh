import {useNavigate} from "react-router-dom";
import {MathJax, MathJaxContext} from "better-react-mathjax"
import React from "react";
import axios from "axios";
import "./IntegralPageStyles.css"
const config = {
    loader: { load: ["input/tex", "output/chtml"] }
};

function IntegralPage() {
    const navigate = useNavigate();
    const [functionInfo, setFunctionInfo] = React.useState(["",""]);
    const [lowerBoundary, setLowerBoundary] = React.useState(0);
    const [upperBoundary, setUpperBoundary] = React.useState(0);
    const [precision, setPrecision] = React.useState(0);
    const [method, setMethod] = React.useState("");
    const [result, setResult] = React.useState(0.0);
    const [n, setN] = React.useState(0);
    const handleSubmit = async () => {
        if(isNaN(Number(lowerBoundary)) || lowerBoundary === "" || isNaN(Number(upperBoundary)) || upperBoundary === "" || isNaN(Number(precision)) || precision === "" || Number(precision) < 0){
            console.log(lowerBoundary, typeof  lowerBoundary);
            console.log(upperBoundary, typeof  upperBoundary);
            console.log(precision, typeof  precision);
            alert("Incorrect inputs");
            return;
        }
        var res = await axios.post("http://localhost:51161/integral/data",
            JSON.stringify({functionInfo: functionInfo, precision: precision,
                lowerBoundary: lowerBoundary, upperBoundary: upperBoundary, method: method}),
            {headers: {'Content-Type': 'application/json'}});
        if (res.data.status !== "OK") {
            setResult(res.data.result);
            setN(res.data.divisionNumber);
        } else {
            alert("Invalid inputs")
        }
    }
    return (
        <MathJaxContext config={config}>
        <div className="integralPageWrapper">
            <div className="dataDiv">
                <label>Choose the function for integrating:</label>
                <div className="functionSelection">
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={JSON.stringify(["LINEAR", "x"])}
                               onChange={(e) => setFunctionInfo(JSON.parse(e.target.value))}/>
                        <MathJax inline>{"\\(x\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={JSON.stringify(["EXPONENTIAL", "e^x"])}
                               onChange={(e) => setFunctionInfo(JSON.parse(e.target.value))}/>
                        <MathJax inline>{"\\(e^x\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={JSON.stringify(["LOGARITHMIC", "\\ln(x)"])}
                               onInput={(e) => setFunctionInfo(JSON.parse(e.target.value))}/>
                        <MathJax inline>{"\\(\ln(x)\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={JSON.stringify(["SINE", "\\sin(x)"])}
                               onInput={(e) => setFunctionInfo(JSON.parse(e.target.value))}/>
                        <MathJax inline>{"\\(\sin(x)\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={JSON.stringify(["POLYNOM", "(2x^3-3x^2+5x-9)"])}
                               onInput={(e) => setFunctionInfo(JSON.parse(e.target.value))}/>
                        <MathJax inline>{"\\(2x^3-3x^2+5x-9\\)"}</MathJax>
                    </label>
                </div>

                <div className={"boundarySelection"}>
                    <div className={"boundaryDiv"}>
                        <label className={"boundaryLabel"}>
                            Lower integration boundary:
                        </label>
                        <input type={"number"} className={"boundaryInput"} value={lowerBoundary}
                                       onChange={(e) => setLowerBoundary(e.target.value)}/>
                    </div>
                    <div className={"boundaryDiv"}>
                        <label className={"boundaryLabel"}>
                            Upper integration boundary:
                        </label>
                        <input type={"number"} className={"boundaryInput"} value={upperBoundary}
                                       onChange={(e) => setUpperBoundary(e.target.value)}/>
                    </div>
                    <div className={"boundaryDiv"}>
                        <label className={"boundaryLabel"}>
                            Precision:
                        </label>
                        <input type={"number"} className={"boundaryInput"} value={precision}
                                       onChange={(e) => setPrecision(e.target.value)}/>
                    </div>
                </div>
                <div className={"intPreviewDiv"}>
                    <label className={"intPreviewLabel"}>
                        Integral preview:
                    </label>
                    <MathJax dynamic inline>
                        {`\\(\\int\\limits_{${lowerBoundary}}^{${upperBoundary}}{${functionInfo[1]}dx} \\)`}</MathJax>
                </div>
                <label>Choose the computation method:</label>
                <div className={"methodWrapper"}>
                    <div className={"methodDiv"}>
                        <label className={"methodLabel"}>
                            <input type={"radio"} name={"method"} value={"LEFT_RECT"}
                                   onChange={(e) => setMethod(e.target.value)}/>
                            Left rectangle method
                        </label>
                    </div>
                    <div className={"methodDiv"}>
                        <label className={"methodLabel"}>
                            <input type={"radio"} name={"method"} value={"RIGHT_RECT"}
                                   onChange={(e) => setMethod(e.target.value)}/>
                            Right rectangle method
                        </label>
                    </div>
                    <div className={"methodDiv"}>
                        <label className={"methodLabel"}>
                            <input type={"radio"} name={"method"} value={"MID_RECT"}
                                   onChange={(e) => setMethod(e.target.value)}/>
                            Midpoint rectangle method
                        </label>
                    </div>
                    <div className={"methodDiv"}>
                        <label className={"methodLabel"}>
                            <input type={"radio"} name={"method"} value={"TRAPEZOID"}
                                   onChange={(e) => setMethod(e.target.value)}/>
                            Trapezoid method
                        </label>
                    </div>
                    <div className={"methodDiv"}>
                        <label className={"methodLabel"}>
                            <input type={"radio"} name={"method"} value={"SIMPSON"}
                                   onChange={(e) => setMethod(e.target.value)}/>
                            Simpson method
                        </label>
                    </div>
                </div>
                <div className={"submitDiv"}>
                    <button type={"button"} onClick={(e) => handleSubmit()}>submit</button>
                </div>
                <div className={"answerDiv"}>
                    <div>
                        <label>
                            Result of computation: {result}
                        </label>
                    </div>
                        <div>
                            <label>
                                Amount of divisions: {n}
                            </label>
                        </div>
                    </div>
                </div>

            <button type={"button"} onClick={(e) => navigate("/")}>go back</button>
        </div>
        </MathJaxContext>
    )
};

export {IntegralPage};