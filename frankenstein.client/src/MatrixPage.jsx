import React, { useEffect, useState } from "react";
import axios from "axios";
import {useNavigate} from "react-router-dom";
import "./assets/CoefficientTable.css"
function MatrixPage() {
    const [matrix, setMatrix] = useState([[0, 0], [0, 0]]);
    const [displayValue, setValue] = useState(() => { return localStorage.getItem("screamer") || 0; });

    const [answerVector, setAnswerVector] = useState([0.0]);
    const [answerNorm, setAnswerNorm] = useState(0.0);
    const [iterations, setIterations] = useState(0);
    const [errors, setErrors] = useState([0.0]);

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
        updatedMatrix[rowIndex][colIndex] = Number(value);
        setMatrix(updatedMatrix)
    }
    const handleFileChange = (e) => {
        setFile(e.target.files[0]);
    }

    const setMatrixSize = (value) => {
        if (value > 20 || value < 1) {
            return;
        }
        var rowCount = matrix.length;
        var updatedMatrix = matrix;
        var updatedValues = values;
        if (rowCount < value) {
            while (rowCount < value) {
                updatedMatrix = updatedMatrix.map((row) => [...row, 0]);
                var newRow = new Array(updatedMatrix[0].length).fill(0);
                updatedMatrix = [...updatedMatrix, newRow];
                updatedValues = [...updatedValues, 0.0];
                rowCount++;
            }
            setMatrix(updatedMatrix);
            setValues(updatedValues);
        } else if (rowCount > value) {
            updatedMatrix = updatedMatrix.slice(0, -(rowCount-value));
            updatedMatrix = updatedMatrix.map((row) => row.slice(0, -(rowCount-value)));
            setMatrix(updatedMatrix);
            setValues(values.slice(0,-(rowCount - value)));
        } else {
            setMatrix(updatedMatrix);
        }
    }

    const handleTableUpload = async () => {
        if (!checkMatrix(matrix)) {
            navigate("/screamer");
            return;
        }
        const result = await axios.post('http://localhost:51161/matrix/table', JSON.stringify({coefficients: matrix, values: values, precision: precision}), {headers: {'Content-Type': 'application/json'}});
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
        const result = await axios.post('http://localhost:51161/matrix/file', formData);
        checkResult(result.data);
    }

    const checkResult = (data) => {
        if(data.status === "OK") {
            setAnswerVector(data.solution);
            setAnswerNorm(data.norm);
            setIterations(data.iterationCount);
            setErrors(data.errors);
        } else {
            localStorage.setItem("error", data.message)
            switch(data.status) {
                case "NO_DIAGONAL_DOMINANCE":{
                    localStorage.setItem("screamer", 1);
                navigate("/screamer");
                break;}
                case "ERROR":{
                    localStorage.setItem("screamer", 2);
                    navigate("/screamer");
                    return;}
                case "ITERATION_LIMIT_EXCEEDED":{
                    localStorage.setItem("screamer", 3);
                    navigate("/screamer");
                    return;
                }
                case "INVALID_MATRIX_ERROR":{
                    localStorage.setItem("screamer", 4);
                    navigate("/screamer");
                    return;
                }
                case "FILE_NOT_FOUND":{
                    localStorage.setItem("screamer", 5);
                    navigate("/screamer");
                    return;
                }
                case "EMPTY_FILE":{
                    localStorage.setItem("screamer", 6);
                    navigate("/screamer");
                    return;
                }
                default:
                    localStorage.setItem("screamer", 1);
                    return;
            }
        }
    }
    const checkMatrix = (matrix) => {
        if (precision < 0) {return false;}
        for (let i = 0; i < matrix.length; i++) {
            for (let j = 0; j < matrix[i].length; j++) {
                if (typeof matrix[i][j] !== "number") {
                    localStorage.setItem("error", "Invalid inputs");
                    localStorage.setItem("screamer", 1);
                    return false;
                }
            }
        }
        return true;
    }
    useEffect(() => {
        const timer = setTimeout(() => {
            setShowLastFrame(true);
        }, 990);
        return () => clearTimeout(timer); // Cleanup timer on unmount
    },);


    return (
        <div className="coefficient-table-container" style={{
            backgroundImage: `url(${showLastFrame ? "../../public/last_monitor_frame.png" : "../../public/FNAF2TheMonitor.webp"})`,
            width: "100%",
            height: "100%",
            backgroundSize: "contain",
            backgroundRepeat: "no-repeat",
            backgroundColor: "black",
            backgroundPosition: "center",
        }}>
            <table className="coefficient-table">
                <tbody>
                    {matrix.map((row, rowIndex) => (
                        <tr key={rowIndex}>
                            {row.map((value, colIndex) => (
                                <td key={colIndex}>
                                    {colIndex!==0 && (<span style={{color:"white"}}>+</span>)}
                                    <input
                                        type={"number"}
                                        value={value}
                                        onChange={(e) =>
                                            handleInputChange(rowIndex, colIndex, e.target.value)
                                        }
                                    />
                                    <span style={{color:"white"}}>
                                        x<sub>{rowIndex+1}{colIndex+1}</sub>
                                    </span>
                                </td>
                            ))}
                        </tr>
                    ))}
                </tbody>
                <tbody>
                {values.map((item,itemIndex) => (
                    <tr key={itemIndex}>
                        <span style={{color:"white"}}>=</span>
                        <input type={"number"} value={item} onChange={(e) => handleResChange(itemIndex, e.target.value)} />
                    </tr>
                ))}
                </tbody>
            </table>
            <div style={{flexDirection: "row", justifyContent: "space-between"}}>
                <span style={{color: "white"}}>Matrix size:</span>
                <input type="number" onChange={(e) => setMatrixSize(e.target.value)} defaultValue={"dim"}/>
                <span style={{color: "white"}}>Precision:</span>
                <input type="number" onChange={(e) => setPrecision(e.target.value)}/>
            </div>
            <button onClick={handleTableUpload}>Submit</button>
            <input type={"file"} onChange={handleFileChange}/>
            <button type={"button"} onClick={handleFileUpload}>Submit file</button>

            <div>
                <span className="dataSpan">-----Solution data-----</span>
                <div className="dataDiv">
                    <span className="dataSpan">Unknown values vector:</span>
                    <div className="rowDiv">
                        {answerVector.map((value, index) => (
                            <span key={index} className="dataSpan">{value}&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        ))}
                    </div>
                </div>
                <div className="dataDiv">
                    <span className="dataSpan">Matrix Norm:</span>
                    <span className="dataSpan">{answerNorm}</span>
                </div>
                <div className="dataDiv">
                    <span className="dataSpan">Amount of iterations:</span>
                    <span className="dataSpan">{iterations}</span>
                </div>
                <div className="dataDiv">
                    <span className="dataSpan">Errors vector:</span>
                    <div className="rowDiv">
                        {errors.map((value, index) => (
                            <span key={index} className="dataSpan">{value}&nbsp;&nbsp;&nbsp;&nbsp;</span>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    )
}

export {MatrixPage};