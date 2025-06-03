import {useNavigate} from "react-router-dom";
import {useState, useEffect, useRef} from "react";
import "./MainPageStyles.css"
import {animated, useSpring} from "react-spring"
import {MatrixPage} from "@/MatrixPage.jsx";
import {IntegralPage} from "@/IntegralPage.jsx";
import {ApproximationPage} from "@/ApproximationPage.jsx";
import {InterpolationPage} from "@/InterpolationPage.jsx";
import {DiffPage} from "@/DiffPage.jsx";
import {NonlinearPage} from "@/NonlinearPage.jsx";

function MainPage() {
    const navigate = useNavigate();
    const [x, setX] = useState(0);
    const [camsOpen, setCamsOpen] = useState(true);
    const [currCam, setCurrCam] = useState("");
    const [currCamPic, setCurrCamPic] = useState("fnaf_office.png");

    const styles = useSpring({
        backgroundPositionX: `${x}%`, // Dynamically adjust the horizontal position
        config: { tension: 120, friction: 14 }, // Smooth easing
    })

    const AnimatedDiv = animated.div;


    const handleMouseMove = (e) => {
        const {left, width} = e.currentTarget.getBoundingClientRect();
        const mouseX = e.clientX - left;
        const relativeX = (mouseX/width)*100;
        setX(relativeX);
    }
    const handleCamDivHover = (e) => {
        console.log(camsOpen)
       setCamsOpen(!camsOpen);
    }
    const handleCamClick = (e) => {
        setCurrCam(e.target.value);
    }
    const determineShownBackground = () => {
        let name = currCam;
        switch(name) {
            case "lab1": {
                return "lab_1.png";
            }
            case "lab2": {
                return "lab2.gif";
            }
            case "lab3": return <IntegralPage/>;
            case "lab4": return <ApproximationPage/>;
            case "lab5": return <InterpolationPage/>;
            case "lab6": {
                return "fnaf_office.png"
            }
            default: return "you.gif";
        }
    }
    const determineShownComponent = () => {
        let name = currCam;
        switch(name) {
            case "lab1": {
                return <MatrixPage/>;
            }
            case "lab2": {
                return <NonlinearPage/>;
            }
            case "lab3": return <IntegralPage/>;
            case "lab4": return <ApproximationPage/>;
            case "lab5": return <InterpolationPage/>;
            case "lab6": {
                return <DiffPage/>;
            }
        }
    }
    return (
        <div className={"mainPage"} onMouseMove={handleMouseMove}>
            <AnimatedDiv className={"background"}
                         style={{
                             backgroundImage: `url(${determineShownBackground()})`,
                             ...styles}}
                         hidden={!camsOpen}
            />
            {camsOpen ? (
                <div className={"cams"}>
                    <div className={"cams"} hidden={!camsOpen}>
                        <img src="/itmo_map.png"/>
                    </div>
                    <button id={"lab1"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab1"}>lab1
                    </button>
                    <button id={"lab2"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab2"}>
                        lab2
                    </button>
                    <button id={"lab3"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab3"}>
                        lab3
                    </button>
                    <button id={"lab4"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab4"}>
                        lab4
                    </button>
                    <button id={"lab5"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab5"}>
                        lab5
                    </button>
                    <button id={"lab6"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"lab6"}>
                        lab6
                    </button>
                    <button id={"office"} type="button" className="camButton" onClick={(e) => setCurrCam(e.target.value)}
                            value={"you"}>
                        you
                    </button>
                </div>
            ) : null}

            {currCam !== "" && <div>{determineShownComponent()}</div>}


            <div className={"camUp"} onMouseOver={handleCamDivHover}>
                vvv
            </div>
        </div>
    )
}

export {MainPage};