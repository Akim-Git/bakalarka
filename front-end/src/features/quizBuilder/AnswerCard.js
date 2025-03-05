import React from "react";
import './QuizBuilder';

const AnswerCard = ({ answer, type, onAnswer, userInput, onInputChange }) => {
    return (
        <div className="answer-card">
            {type === "contains" ? (
                <>
                    <input
                        type="text"
                        placeholder="odpověď"
                        value={userInput}
                        onChange={onInputChange}
                    />
                    <button onClick={onAnswer}>Submit</button> 
                </>
            ) : (
                <button onClick={onAnswer}>
                    {answer}
                </button>
            )}
        </div>
    );
}

export default AnswerCard;
