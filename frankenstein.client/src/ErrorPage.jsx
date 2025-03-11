import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
function ErrorPage() {
    const error = localStorage.getItem('error');
    console.log(error)
    const navigate = useNavigate();
    const handleClick = (e) => {
        navigate("/");
    }

    return (
        <div
            style={{
                width: "100vw",
                height: "100vh",
                display: "flex",
                flexDirection: "column",
                justifyContent: "center",
                alignItems: "center",
                backgroundColor: "black",
                margin: 0,
                overflow: "hidden",
                backgroundImage: "url('../public/game_over_static.webp')"
            }}
        >
            <span style={{color: "white", fontSize: "30px" }}>{error}</span>
            <button type="button" onClick={handleClick}>Try again</button>
        </div>
    )
}
export default ErrorPage;