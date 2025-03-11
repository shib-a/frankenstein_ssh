import {Route, BrowserRouter as Router, Routes} from "react-router-dom";
import './App.css';
import { CoefficientTable } from './CoefficientTable';
import ScreamerPage from "@/assets/ScreamerPage.jsx";
import ErrorPage from "@/ErrorPage.jsx";

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<CoefficientTable/>}/>
                <Route path="/screamer" element={<ScreamerPage/>}/>
                <Route path="/error" element={<ErrorPage/>}/>
            </Routes>
        </Router>
    );
}

export default App;