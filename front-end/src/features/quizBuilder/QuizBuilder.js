import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import AnswerCard from './AnswerCard';
import defaultImage from '../quizBuilder/sign.jpg';

const QuizBuilder = () => {
    const { id } = useParams();
    const [loading, setLoading] = useState(true);
    const [questions, setQuestions] = useState([]);
    const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
    const [score, setScore] = useState(0);
    const [userInput, setUserInput] = useState("");

    useEffect(() => {
        const getQuestions = async () => {
            try {
                const response = await fetch(`https://localhost:7006/api/Game/${id}/questions-solo`, {
                    method: 'GET',
                    credentials: 'include'
                });
                const data = await response.json();

                console.log(data);

                if (response.ok) {
                    setQuestions(data);
                } else {
                    console.error('Error fetching data:', data);
                }
            } catch (error) {
                console.error('Error:', error);
            } finally {
                setLoading(false);
            }
        };

        getQuestions();
    }, [id]);

    const handleAnswer = (isCorrect) => {
        if (isCorrect) {
            setScore(score + 1);
        }
        setCurrentQuestionIndex(currentQuestionIndex + 1);
        setUserInput("");
    };

    const checkContainsAnswer = () => {
        const currentQuestion = questions[currentQuestionIndex];
        const isUserInputCorrect = currentQuestion.answers.some(answer => 
            answer.text.trim().toLowerCase() === userInput.trim() && answer.isCorrect
        );
    
        handleAnswer(isUserInputCorrect);
    };
    
    
    if (loading) {
        return <div>Loading...</div>;
    }

    if (currentQuestionIndex >= questions.length) {
        return (
            <div>
                <h1>Quiz Completed!</h1>
                <p>Your score: {score} out of {questions.length}</p>
            </div>
        );
    }

    const currentQuestion = questions[currentQuestionIndex];

    
    const getImageSrc = (imageData, imageType) => {
        return `data:${imageType};base64,${imageData}`;
    };
    
    const imageSrc = currentQuestion.imageDataQuestion 
        ? getImageSrc(currentQuestion.imageDataQuestion, 'image/jpeg') 
        : defaultImage;
    

    return (
        <div>
            <img src={imageSrc} alt="Question"/>
            <h1>{currentQuestion.text}</h1>
            <p>Type of Question: {currentQuestion.type}</p> 
            <div>
                {currentQuestion.questionType === "contains" ? (
                    <AnswerCard
                        type="contains"
                        userInput={userInput}
                        onInputChange={(e) => setUserInput(e.target.value)}
                        onAnswer={checkContainsAnswer}
                    />
                ) : (
                    currentQuestion.answers.map(answer => (
                        <AnswerCard
                            key={answer.id}
                            answer={answer.text}
                            onAnswer={() => handleAnswer(answer.isCorrect)}
                            type={currentQuestion.type}
                        />
                    ))
                )}
            </div>
        </div>
    );
};

export default QuizBuilder;
