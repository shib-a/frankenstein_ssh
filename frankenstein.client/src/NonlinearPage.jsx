import {useNavigate} from "react-router-dom";
import {MathJax, MathJaxContext} from "better-react-mathjax"
import React, {useEffect, useState} from "react";
import axios from "axios";
import "./NonlinearPageStyles.css"
const config = {
    loader: { load: ["input/tex", "output/chtml"] }
};

function NonlinearPage() {
    const navigate = useNavigate();
    const [functionInfo, setFunctionInfo] = React.useState("");
    const [lowerBoundary, setLowerBoundary] = React.useState(0);
    const [upperBoundary, setUpperBoundary] = React.useState(0);
    const [precision, setPrecision] = React.useState(0);
    const [method, setMethod] = React.useState("");
    const [result, setResult] = React.useState(0.0);
    const [n, setN] = React.useState(0);
    const [image, setImage] = useState(null);
    const [error, setError] = useState(null);
    const [type, setType] = React.useState("EQUATION");
    const [x0, setX0] = React.useState(0);
    const [y0, setY0] = React.useState(0);
    const [currentImage, setCurrentImage] = React.useState("public/room/room_mid.png");
    const [clickCount, setClickCount] = React.useState(0);

    const sys1 = `
  \\[
  \\left\\{
    \\begin{array}{l}
      x + y = 5 \\\\
      2x - y = 3
    \\end{array}
  \\right.
  \\]
`;

    const handleSubmit = async () => {
        if(isNaN(Number(lowerBoundary)) || lowerBoundary === "" || isNaN(Number(upperBoundary)) || upperBoundary === "" || isNaN(Number(precision)) || precision === "" || Number(precision) < 0){
            console.log(lowerBoundary, typeof  lowerBoundary);
            console.log(upperBoundary, typeof  upperBoundary);
            console.log(precision, typeof  precision);
            alert("Incorrect inputs");
            return;
        }
        var res = await axios.post("http://localhost:51161/nonlinear/data",
            JSON.stringify({functionInfo: functionInfo, precision: Number(precision),
                lowerBoundary: Number(lowerBoundary), upperBoundary: Number(upperBoundary), method: method, type: type}),
            {headers: {'Content-Type': 'application/json'}});
        if (res.data.status !== "OK") {
            setResult(res.data.root);
            setError(res.data.message);
            setImage(res.data.plot)
            setN(res.data.iterationCount);
        } else {
            alert("Invalid inputs")
        }
    }
    const handleRoomClick = (e) =>{
        const rect = e.target.getBoundingClientRect();
        const clickX = e.clientX - rect.left; // X-coordinate relative to the image
        const clickY =  rect.height - e.clientY;
        console.log(rect.top, rect.height);
        console.log(clickY);
        const imageWidth = rect.width;
        const imageHeight = rect.height;
        let newImage = "";
        if (currentImage === "public/room/door_right_dark.png" || currentImage === "public/room/closet.png" || currentImage === "public/room/door_left_dark.png") {
            if (!(clickY < imageHeight/4)) {
                return;
            } else {
                newImage = "public/room/room_mid.png";
            }
        }
        if (clickX < imageWidth / 4) {
           newImage =  "public/room/room_left.png";
        } else if (clickX > imageWidth / 4 * 3) {
            newImage = "public/room/room_right.png";
        } else {
            newImage = "public/room/room_mid.png";
        }
        if (currentImage === newImage) {
            switch (currentImage) {
                case "public/room/room_right.png": newImage = "public/room/door_right_dark.png"; break;
                case "public/room/room_left.png": newImage = "public/room/door_left_dark.png"; break;
                case "public/room/room_mid.png": newImage = "public/room/closet.png"; break;
            }

        }
        setCurrentImage(newImage)
    }
    const determineShownBackground = (e) =>{
        return currentImage;
    }
    useEffect(() => {

    }, [error])
    return (
        <MathJaxContext config={config}>
            <div className={"room"}  onClick={handleRoomClick} style={{backgroundImage: `url(${determineShownBackground()})`}}>


            <div className="integralPageWrapper">
                <div className="dataDiv">
                    {/*<div className="functionSelection">*/}
                        {/*<input type={"radio"} name={"type"} value={"EQUATION"}*/}
                        {/*       onChange={(e) => setType(e.target.value)}/>*/}
                        {/*<p>Nonlinear equation</p>*/}
                        {/*<input type={"radio"} name={"type"} value={"SYSTEM"}*/}
                        {/*       onChange={(e) => setType(e.target.value)}/>*/}
                        {/*<p>Nonlinear system of equations</p>*/}
                        {currentImage === "public/room/door_right_dark.png" &&
                            (<div className={"darkassDiv"}>
                                <div className="functionSelection">
                                    <label className={"functionLabel"}>
                                        <input type={"radio"} name={"function"} value={"ONE"}
                                               onChange={(e) => setFunctionInfo(e.target.value)}/>
                                        <MathJax inline>{"\\(-1.8x^3-2.94x^2+10.37x+5.38\\)"}</MathJax>
                                    </label>
                                    <label className={"functionLabel"}>
                                        <input type={"radio"} name={"function"} value={"TWO"}
                                               onChange={(e) => setFunctionInfo(e.target.value)}/>
                                        <MathJax inline>{"\\(x^3-x+4\\)"}</MathJax>
                                    </label>
                                    <label className={"functionLabel"}>
                                        <input type={"radio"} name={"function"} value={"THREE"}
                                               onInput={(e) => setFunctionInfo(e.target.value)}/>
                                        <MathJax inline>{"\\(\arctg(x)\\)"}</MathJax>
                                    </label>
                                </div>
                                <div className={"boundarySelection"}>
                                    <div className={"boundaryDiv"}>
                                        <label className={"boundaryLabel"}>
                                            Lower boundary:
                                        </label>
                                        <input type={"number"} className={"boundaryInput"} value={lowerBoundary}
                                               onChange={(e) => setLowerBoundary(e.target.value)}/>
                                    </div>
                                    <div className={"boundaryDiv"}>
                                        <label className={"boundaryLabel"}>
                                            Upper boundary:
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
                                <label className={"methodLabel"}>Choose the computation method:</label>
                                <div className={"methodWrapper"}>
                                    <div className={"methodDiv"}>
                                        <label className={"methodLabel"}>
                                            <input type={"radio"} name={"method"} value={"CHORD"}
                                                   onChange={(e) => setMethod(e.target.value)}/>
                                            Chord method
                                        </label>
                                    </div>
                                    <div className={"methodDiv"}>
                                        <label className={"methodLabel"}>
                                            <input type={"radio"} name={"method"} value={"NEWTON"}
                                                   onChange={(e) => setMethod(e.target.value)}/>
                                            Newton method
                                        </label>
                                    </div>
                                    <div className={"methodDiv"}>
                                        <label className={"methodLabel"}>
                                            <input type={"radio"} name={"method"} value={"SIMPLE"}
                                                   onChange={(e) => setMethod(e.target.value)}/>
                                            Simple iteration method
                                        </label>
                                    </div>
                                </div>
                                <div className={"submitDiv"}>
                                    <button type={"button"} onClick={(e) => handleSubmit()}>submit</button>
                                </div>
                            </div>)}
                    {currentImage === "public/room/door_left_dark.png" &&
                        (<div className={"darkassDiv"}>
                            <div className="functionSelection">
                                <label className={"functionLabel"}>
                                    <input type={"radio"} name={"function"} value={"ONE"}
                                           onChange={(e) => setFunctionInfo(e.target.value)}/>
                                    <MathJax inline>{sys1}</MathJax>
                                </label>
                                <label className={"functionLabel"}>
                                    <input type={"radio"} name={"function"} value={"TWO"}
                                           onChange={(e) => setFunctionInfo(e.target.value)}/>
                                    <MathJax inline>{sys1}</MathJax>
                                </label>
                            </div>

                            <div className={"boundarySelection"}>
                                <div className={"boundaryDiv"}>
                                    <label className={"boundaryLabel"}>
                                        x0:
                                    </label>
                                    <input type={"number"} className={"boundaryInput"} value={lowerBoundary}
                                           onChange={(e) => setLowerBoundary(e.target.value)}/>
                                </div>
                                <div className={"boundaryDiv"}>
                                    <label className={"boundaryLabel"}>
                                        y0:
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
                            <div className={"submitDiv"}>
                                <button type={"button"} onClick={(e) => handleSubmit()}>submit</button>
                            </div>
                        </div>)
                    }
                </div>
                {currentImage === "public/room/closet.png" &&
                    (<div className={"answerDiv"}>
                    {error === null ? "" : <p style={{color: "black"}}>Error: {error}</p>}
                        {image ? <img src={`data:image/png;base64,${image}`} alt="image"/> : ""}
                        <div>
                            <label>
                                Result of computation: {result}
                            </label>
                        </div>
                        <div>
                            <label>
                                Amount of iterations: {n}
                            </label>
                        </div>
                    </div>)}
                </div>
            {/*</div>*/}
            </div>
        </MathJaxContext>
    )
}

export {NonlinearPage};