import {Route, BrowserRouter as Router, Routes} from "react-router-dom";
import './App.css';
import { CoefficientTable } from './CoefficientTable';
import {MainPage} from "@/MainPage.jsx";
import ScreamerPage from "@/assets/ScreamerPage.jsx";
import ErrorPage from "@/ErrorPage.jsx";
import {IntegralPage} from "@/IntegralPage.jsx";
import {ApproximationPage} from "@/ApproximationPage.jsx";
import {DiffPage} from "@/DiffPage.jsx";
import {InterpolationPage} from "@/InterpolationPage.jsx";

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<MainPage/>}/>
                <Route path="/lab1" element={<CoefficientTable/>}/>
                <Route path="/lab3" element={<IntegralPage/>}/>
                <Route path="/lab4" element={<ApproximationPage/>}/>
                <Route path="/lab5" element={<InterpolationPage/>}/>
                <Route path="/lab6" element={<DiffPage/>}/>
                <Route path="/screamer" element={<ScreamerPage/>}/>
                <Route path="/error" element={<ErrorPage/>}/>
            </Routes>
        </Router>
    );
}

export default App;