import React, { useEffect, useState } from "react";
import axios from "axios";
import {useNavigate} from "react-router-dom";
import {MathJax, MathJaxContext} from "better-react-mathjax";
import "./DiffPageStyles.css"
const config = {
    loader: { load: ["input/tex", "output/chtml"] }
};
function DiffPage() {

    const [image, setImage] = useState(null);
    const [func, setFunc] = useState(null);
    const [eps, setEps] = useState(0.01);
    const [x0, setX0] = useState(0);
    const [y0, setY0] = useState(1);
    const [xn, setXn] = useState(1);
    const [h, setH] = useState(0.1);

    const [answer, setAnswer] = useState(null);
    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const handleTableUpload = async () => {
        if(isNaN(Number(x0)) || isNaN(Number(y0)) || isNaN(Number(xn)) || isNaN(Number(eps)) || isNaN(Number(h))) {
            alert("Please enter valid number");
            return;
        }
        const result = await axios.post('http://localhost:51161/diff/data',
            JSON.stringify({function: func, x0: Number(x0), xn: Number(xn), y0: Number(y0), eps: Number(eps), h: Number(h)}), {headers: {'Content-Type': 'application/json'}});
        checkResult(result.data);
    }

    const checkResult = (data) => {
        if(data.errorMessage !== null) {
            console.log(data.errorMessage);
            setError(data.errorMessage)
            setFunc(null);
        } else {
            setImage(data.plot);
            setAnswer(data.solutions);
        }
    }

    useEffect(() => {

    }, [image])
    return (
        <MathJaxContext config={config}>
        <div className={"approximationPage"}>
            <div className={"dataDiv"}>
                <label>Choose the function for integrating:</label>
                <div className="functionSelection">
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={"LINEAR"}
                               onChange={(e) => setFunc(e.target.value)}/>
                        <MathJax inline>{"\\(y' = x + y\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={"QUADRATIC"}
                               onChange={(e) => setFunc(e.target.value)}/>
                        <MathJax inline>{"\\(y' = y + (1+x)y^2\\)"}</MathJax>
                    </label>
                    <label className={"functionLabel"}>
                        <input type={"radio"} name={"function"} value={"EXPONENTIAL"}
                               onInput={(e) => setFunc(e.target.value)}/>
                        <MathJax inline>{"\\(y' = e^x\\)"}</MathJax>
                    </label>
                </div>
                <span style={{color: "black"}}> Starting x0 of the interval: </span>
                <input type="text" className={"textInput"} onChange={(e) => setX0(e.target.value)}/>
                <span style={{color: "black"}}> Last xn of the interval: </span>
                <input type="text" className={"textInput"} onChange={(e) => setXn(e.target.value)}/>
                <span style={{color: "black"}}> Function's value at x0 (y0): </span>
                <input type="text" className={"textInput"} onChange={(e) => setY0(e.target.value)}/>
                <span style={{color: "black"}}> Step (h): </span>
                <input type="text" className={"textInput"} onChange={(e) => setH(e.target.value)}/>
                <span style={{color: "black"}}> Precision: </span>
                <input type="text" className={"textInput"} onChange={(e) => setEps(e.target.value)}/>
            </div>
            <div className={"submitDiv"}>
                <button type={"button"} onClick={handleTableUpload}>Submit</button>
                <button type={"button"} onClick={(e) => {navigate("/")}}>Go back</button>
            </div>
            <div className={"answerDiv"} style={{color: "black"}}>
                {error === null ? "" : <p style={{color: "black"}}>Error: {error}</p>}
                {image ? <img src={`data:image/png;base64,${image}`} alt="image"/> : ""}
                <div>
                    {answer !== null ? <div>
                        <div>
                            <div>
                                <label style={{color: "black"}}>
                                    Euler:
                                </label>
                                {" "}{answer[0]}
                            </div>
                            <div>
                                <label style={{color: "black"}}>
                                    Modified Euler:
                                </label>
                                {" "}{answer[1]}
                            </div>
                            <div>
                                <label style={{color: "black"}}>
                                    Milne:
                                </label>
                                {" "}{answer[2]}
                            </div>
                        </div>
                    </div> : ""}
                </div>
            </div>

        </div>
        </MathJaxContext>
    )
}

export {DiffPage};