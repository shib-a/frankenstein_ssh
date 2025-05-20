import React, { useEffect, useState } from "react";
import axios from "axios";
import {useNavigate} from "react-router-dom";
import "./ApproximationPageStyles.css";
import {MathJax, MathJaxContext} from "better-react-mathjax";
const config = {
    loader: { load: ["input/tex", "output/chtml"] }
};
function InterpolationPage() {
    const [matrix, setMatrix] = useState([["0", "0", "0"], ["0", "0", "0"]]);
    const [size, setSize] = useState(4)
    const [targetX, setTargetX] = React.useState(0);

    const [image, setImage] = useState(null);
    const [func, setFunc] = useState(null);
    const [error, setError] = useState(null);
    const [r, setR] = useState(null);

    const [values, setValues] = useState([0.0, 0.0]);
    const [file, setFile] = useState(null);
    const navigate = useNavigate();

    const handleResChange = (itemIndex,value) => {
        const updatedValues = [...values];
        updatedValues[itemIndex] = Number(value);
        setValues(updatedValues)
    }
    const handleInputChange = (rowIndex, colIndex, value) => {
        const updatedMatrix = [...matrix];
            updatedMatrix[rowIndex][colIndex] = value;
        setMatrix(updatedMatrix)
    }
    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    }
    useEffect(() => {
        if(error){
            const timer = setTimeout(() => {
                setError(null);
            }, 5000);
            return () => clearTimeout(timer);
        }
    }, [error])
    const setMatrixSize = (value) => {
        if (value > 12 || value < 3) {
            return;
        }
        var rowCount = matrix[0].length;
        var updatedMatrix = matrix;
        var updatedValues = values;
        if (rowCount < value) {
            while (rowCount < value) {
                updatedMatrix = updatedMatrix.map((row) => [...row, 0]);
                updatedValues = [...updatedValues, 0.0];
                rowCount++;
            }
            setMatrix(updatedMatrix);
            setValues(updatedValues);
        } else if (rowCount > value) {
            updatedMatrix = updatedMatrix.map((row) => row.slice(0, -(rowCount-value)));
            setMatrix(updatedMatrix);
            setValues(values.slice(0,-(rowCount - value)));
        } else {
            setMatrix(updatedMatrix);
        }
    }

    const handleTableUpload = async () => {
        const numberMatrix = new Array(matrix.length);
        numberMatrix[0] = new Array(matrix[0].length);
        numberMatrix[1] = new Array(matrix[1].length);
        for (let i = 0; i < matrix.length; i++) {
            for (let j = 0; j < matrix[1].length; j++) {
                console.log(matrix[i][j], typeof(matrix[i][j]));
                if (!isNaN(Number(matrix[i][j]))) {
                    numberMatrix[i][j] = Number(matrix[i][j]);
                } else {
                    setError("Invalid matrix input")
                    return;
                }
            }
        }
        const result = await axios.post('http://localhost:51161/interpolation/data', JSON.stringify({yValues: numberMatrix[1], xValues: numberMatrix[0], targetX: Number(targetX)}), {headers: {'Content-Type': 'application/json'}});
        checkResult(result.data);
    }
    const handleFileUpload = async (e) => {
        e.preventDefault();
        if(!file){
            localStorage.setItem("error", "No file uploaded");
            localStorage.setItem("screamer", 5);
            navigate("/screamer");
            return;
        }
        const formData = new FormData();
        formData.append("file", file);
        const result = await axios.post('http://localhost:51161/approximation/file', formData);
        checkResult(result.data);
    }

    const checkResult = (data) => {
        if(data.errorMessage !== null) {
            console.log(typeof data.errorMessage);
            console.log(data.errorMessage);
            setError(data.errorMessage)
            setFunc(null);
        } else {
            setImage(data.plot);
            setFunc(data.function);
            console.log(data.function);
        }
    }

    useEffect(() => {

    }, [image])
    function roundUp(num, decimalPlaces) {
        const factor = Math.pow(10, decimalPlaces);
        return Math.ceil(num * factor) / factor;
    }
    return (
        <MathJaxContext config={config}>
        <div className={"approximationPage"}>
            <div className={"tableWrapper"}>
            <div className="coefficientTable">
                <div className={"tbody"}>
                {matrix.map((row, rowIndex) => (
                    <div className={"rowDiv"} key={rowIndex}>
                        {rowIndex===0? <label>x</label>: <label>y</label>}
                        {row.map((value, colIndex) => (
                            <div key={colIndex} style={{ display: "inline-block", width: `calc(100% / ${matrix.length})` }}>
                                {colIndex!==0}
                                <input
                                    className={"tableInput"}
                                    type="text"
                                    value={value}
                                    onChange={(e) =>
                                        handleInputChange(rowIndex, colIndex, e.target.value)
                                    }
                                    style={{appearance: "none", width:`calc(100%/${matrix.length})`, gap:`calc(100%/${matrix.length})px`}}
                                />
                            </div>
                        ))}
                    </div>
                ))}
                </div>
            </div>
            </div>
            <div>
                <span style={{color:"black"}}>Amount of values: </span>
                <input type="number" onChange={(e) => setMatrixSize(e.target.value)} defaultValue={3}/>
            </div>
            <div>
                <span style={{color:"black"}}>Target x: </span>
                <input type="number" onChange={(e) => setTargetX(e.target.value)} defaultValue={0}/>
            </div>
            <div className={"answerDiv"} style={{color:"black"}}>
                {error===null ? "" : <p style={{color:"black"}}>Error: {error}</p>}
                {image ? <img src={`data:image/png;base64,${image}`} alt="image" /> : ""}
                <div>
                    {image!=null ? <div>
                        <div>
                        </div>
                        <div>
                            {/*<MathJax dynamic inline>{` \\( S = ${roundUp(deviation,5)}\\)`}</MathJax>*/}
                        </div>
                        <div>
                            {/*<MathJax dynamic inline>{` \\( \\sigma = ${roundUp(mse,5)}\\)`}</MathJax>*/}
                        </div>
                        <div>
                            <label style={{color:"black"}}>
                                Determination coefficient
                            </label>
                            <MathJax dynamic inline>{` \\(R^2 = ${roundUp(r, 5)}\\)`}</MathJax>
                        </div>
                        <div>
                            {/*<label style={{color:"black"}}>*/}
                            {/*    Cooefficients: {coeffs.map((coef, coefIndex) => (<p>{roundUp(coef,5)}</p>))}*/}
                            {/*</label>*/}
                        </div>
                    </div> : ""}
                </div>
            </div>
            <div className={"submitDiv"}>
                <button onClick={handleTableUpload}>Submit</button>
                <input type={"file"} onChange={handleFileChange} style={{color:"black"}}/>
                <button type={"button"} onClick={handleFileUpload}>Submit file</button>
            </div>
        </div>
        </MathJaxContext>
    )
}

export {InterpolationPage};