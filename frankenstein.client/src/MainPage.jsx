import {useNavigate} from "react-router-dom";
import {useState} from "react";
import "./MainPageStyles.css"
import {animated, useSpring} from "react-spring"

function MainPage() {
    const navigate = useNavigate();
    const [x, setX] = useState(0);
    const [camsOpen, setCamsOpen] = useState(true);
    const [currCam, setCurrCam] = useState(0);

    const styles = useSpring({
        backgroundPositionX: `${-x}%`, // Dynamically adjust the horizontal position
        config: { tension: 120, friction: 14 }, // Smooth easing
    })

    const handleMouseMove = (e) => {
        const {left, width} = e.currentTarget.getBoundingClientRect();
        const mouseX = e.clientX - left;
        const relativeX = (mouseX/width)*100;
        setX(relativeX);
    }
    const handleCamDivHover = (e) => {
       setCamsOpen(!camsOpen);
    }

    return (
        <div className={"mainPage"} onMouseMove={handleMouseMove}>
            <animated.div className={"background"}
                          style={{...styles}}
            />
            <div className={"cams"}>
                <div className="button-wrapper">
                    <button type="button" className="button" onClick={(e) => navigate("/lab1")}>lab1</button>
                </div>
                <div className="button-wrapper">
                    <button type="button" className="button" onClick={(e) => navigate("/lab3")}>lab3</button>
                </div>
                <div className="button-wrapper">
                    <button type="button" className="button" onClick={(e) => navigate("/lab4")}>lab4</button>
                </div>
                <div className="button-wrapper">
                    <button type="button" className="button" onClick={(e) => navigate("/lab5")}>lab5</button>
                </div>
                <div className="button-wrapper">
                    <button type="button" className="button" onClick={(e) => navigate("/lab6")}>lab6</button>
                </div>
            </div>
            <div className={"camUp"} onMouseOver={handleCamDivHover}>
                cock
            </div>
        </div>
    )
}

export {MainPage};