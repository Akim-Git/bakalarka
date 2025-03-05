import React from "react";
import './HomeStyle.css';

const QuizCard = ({ title, description, image, id, isAdmin, hasLobby, onDelete, onQuizToLobby, onNavigate}) => {
    return (
        <div className="quiz-card">
            {isAdmin && <button className="admin-indicator" onClick={() => onDelete(id)}>❌</button>}
            <img src={image} alt="Quiz" /> 
            <h3>{title}</h3>
            <p>{description}</p>
            <span style={{display: "none"}} >{id}</span> 
            <span>{id}</span> 
           {!hasLobby &&  <button onClick={() => onNavigate(id)} className="navigate-button"> Hrát kvíz</button>}
           {hasLobby && <button onClick={() => onQuizToLobby(id, title)}>Výbrat kvíz pro lobby</button>}
        </div>
    );
}

export default QuizCard;










