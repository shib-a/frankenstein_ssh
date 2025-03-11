import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
function ScreamerPage() {
    const [displayValue, setDisplayValue] = useState(0);
    const navigate = useNavigate();
    useEffect(() => {
        const storedValue = localStorage.getItem("screamer");
        console.log(typeof storedValue);
        setDisplayValue(storedValue)
        // switch (storedValue) {
        //     case 0: return setDisplayValue(0);
        //     case 1: setDisplayValue(1);
    // }
    }, [])
    useEffect(() => {
        const timer = setTimeout(() => {
            setDisplayValue(0);
            navigate("/error");
        }, 1000);
        return () => clearTimeout(timer);
    }, [navigate]);

    return (
        <div
            style={{
                width: "100vw",
                height: "100vh",
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                // backgroundColor: "black", // Optional: background color to contrast the media
                margin: 0,
                overflow: "hidden"
            }}
        >
            {displayValue === "1" && (<img
                src="../../public/foxyscreamer.gif"
                alt="foxyScreamerGif"
                style={{ width: "100%", height: "auto", objectFit: "cover" }}
            />
            )}
            {displayValue === "2" && (<img
                    src="../../public/wither-bonnie-gif.gif"
                    alt="foxyScreamerGif"
                    style={{ width: "100%", height: "auto", objectFit: "cover" }}
                />
            )}
            {displayValue === "3" && (<img
                    src="../../public/jumpscare-fnaf.gif"
                    alt="foxyScreamerGif"
                    style={{ width: "100%", height: "auto", objectFit: "cover" }}
                />
            )}
            {displayValue === "4" && (<img
                    src="../../public/nightmare-fredbear-fnaf.gif"
                    alt="foxyScreamerGif"
                    style={{ width: "100%", height: "auto", objectFit: "cover" }}
                />
            )}
            {displayValue === "5" && (<img
                    src="../../public/nightmare-fnaf.gif"
                    alt="foxyScreamerGif"
                    style={{ width: "100%", height: "auto", objectFit: "cover" }}
                />
            )}
            {displayValue === "6" && (<img
                    src="../../public/ballora-fnaf.gif"
                    alt="foxyScreamerGif"
                    style={{ width: "100%", height: "auto", objectFit: "cover" }}
                />
            )}
        </div>
    );
}
export default ScreamerPage;