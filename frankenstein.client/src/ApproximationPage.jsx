import React, { useEffect, useState } from "react";
import axios from "axios";
import {useNavigate} from "react-router-dom";
import "./ApproximationPageStyles.css";
import {MathJax, MathJaxContext} from "better-react-mathjax";
const config = {
    loader: { load: ["input/tex", "output/chtml"] }
};
function ApproximationPage() {
    const [matrix, setMatrix] = useState([["0", "0"], ["0", "0"]]);
    const [size, setSize] = useState(4)
    const [displayValue, setValue] = useState(() => { return localStorage.getItem("screamer") || 0; });

    const [image, setImage] = useState(null);
    const [deviation, setDeviation] = useState(null);
    const [func, setFunc] = useState(null);
    const [error, setError] = useState(null);
    const [mse, setMse] = useState(null);
    const [r, setR] = useState(null);
    const [coeffs, setCoeffs] = useState(null);

    const [precision, setPrecision] = useState(0.0);
    const [values, setValues] = useState([0.0, 0.0]);
    const [file, setFile] = useState(null);
    const navigate = useNavigate();
    const [showLastFrame, setShowLastFrame] = useState(false);

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

    const setMatrixSize = (value) => {
        if (value > 12 || value < 8) {
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
                }
            }
        }
        const result = await axios.post('http://localhost:51161/approximation/data', JSON.stringify({yValues: numberMatrix[1], xValues: numberMatrix[0]}), {headers: {'Content-Type': 'application/json'}});
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
            setFunc(null);
        } else {
            setImage(data.plot);
            setFunc(data.function);
            setDeviation(data.deviation);
            setR(data.r2);
            setMse(data.mse);
            console.log(data.function);

        }
    }

    useEffect(() => {

    }, [image])
    return (
        <MathJaxContext config={config}>
        <div className={"approximationPage"}>
            <div className={"tableWrapper"}>
            <div className="coefficientTable">
                <div className={"tbody"}>
                {matrix.map((row, rowIndex) => (
                    <div className={"rowDiv"} key={rowIndex}>
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
                <span >Matrix size:</span>
                <input type="number" onChange={(e) => setMatrixSize(e.target.value)} defaultValue={"dim"}/>
            </div>
            <div className={"answerDiv"}>
                {image ? <img src={`data:image/png;base64,${image}`} alt="image" /> : ""}
                <div>
                    {func!=null ? <div>
                        <div>
                            <label>
                                Best type of approximation - {func}
                            </label>

                        </div>
                        <div>
                            <label>
                                Deviation
                            </label>
                            <MathJax dynamic inline>{` \\( S = ${deviation}\\)`}</MathJax>
                        </div>
                        <div>
                            <label>
                                MSE
                            </label>
                            <MathJax dynamic inline>{` \\( \\sigma = ${mse}\\)`}</MathJax>
                        </div>
                        <div>
                            <label>
                                Determination coefficient
                            </label>
                            <MathJax dynamic inline>{` \\(R^2 = ${r}\\)`}</MathJax>
                        </div>
                    </div> : ""}
                </div>
            </div>
            <button onClick={handleTableUpload}>Submit</button>
            <input type={"file"} onChange={handleFileChange}/>
            <button type={"button"} onClick={handleFileUpload}>Submit file</button>
        </div>
        </MathJaxContext>
    )
}

export {ApproximationPage};