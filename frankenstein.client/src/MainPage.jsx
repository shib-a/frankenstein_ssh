import {useNavigate} from "react-router-dom";

function MainPage() {
    const navigate = useNavigate();
    return (
        <div className="main-page">
            <div className="button-wrapper">
                <button type="button" className="button" onClick={(e) => navigate("/lab1")}>lab1</button>
            </div>
            <div className="button-wrapper">
                <button type="button" className="button" onClick={(e) => navigate("/lab3")}>lab3</button>
            </div>
        </div>
    )
}

export {MainPage};