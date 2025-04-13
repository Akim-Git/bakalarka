import React, { useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Navigate, useNavigate } from 'react-router-dom';
import './CreateQuiz.css';

const CreateQuiz = () => {
    const { isAuthenticated, user } = useAuth();
    const navigate = useNavigate();

    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [quizImage, setQuizImage] = useState(null);
    const [questions, setQuestions] = useState([
        { text: '', TimeForAnswer: "", questionType: '', image: null, answers: [{ text: '', isCorrect: false }, { text: '', isCorrect: false }] }
    ]);
    const [errorMessages, setErrorMessages] = useState(null);

    if (!isAuthenticated) {
        return <Navigate to="/login" />;
    }

    const handleAddQuestion = () => {
        setQuestions([...questions, { text: '', questionType: '', TimeForAnswer: '', image: null, answers: [{ text: '', isCorrect: false }, { text: '', isCorrect: false }] }]);
    };

    const handleDeleteQuestion = (index) => {
        if (questions.length > 1) {
            const updatedQuestions = [...questions];
            updatedQuestions.splice(index, 1);
            setQuestions(updatedQuestions);
        } else {
            alert("Kvíz musí mít alespoň jednu otázku.");
        }
    };

    const handleQuestionChange = (index, value) => {
        setQuestions((prevQuestions) =>
            prevQuestions.map((q, i) => (i === index ? { ...q, text: value } : q))
        );
    };

    const handleQuestionTypeChange = (index, value) => {
        setQuestions((prevQuestions) =>
            prevQuestions.map((q, i) => {
                if (i === index) {
                    let answers;
                    if (value === 'choice') {
                        answers = [{ text: '', isCorrect: false }, { text: '', isCorrect: false }];
                    } else if (value === 'true or false') {
                        answers = [
                            { text: 'Pravda', isCorrect: false },
                            { text: 'Lež', isCorrect: false }
                        ];
                    } else if (value === 'contains') {
                        answers = [{ text: '', isCorrect: true }];
                    }
                    return { ...q, questionType: value, answers };
                }
                return q;
            })
        );
    };

    const handleTimeForAnswerChange = (index, value) => {
        setQuestions((prevQuestions) =>
            prevQuestions.map((q, i) => (i === index ? { ...q, TimeForAnswer: value } : q))
        );
    };

    const handleImageChange = (index, e) => {
        setQuestions((prevQuestions) =>
            prevQuestions.map((q, i) => (i === index ? { ...q, image: e.target.files[0] } : q))
        );
    };

    const handleQuizImageChange = (e) => {
        setQuizImage(e.target.files[0]);
    };

    const handleAddAnswer = (questionIndex) => {
        setQuestions((prevQuestions) => {
            const updatedQuestions = [...prevQuestions];
            const question = updatedQuestions[questionIndex];

            if (question.questionType === 'choice') {
                question.answers.push({ text: '', isCorrect: false });
            }
            return updatedQuestions;
        });
    };

    const handleDeleteAnswer = (questionIndex, answerIndex) => {
        setQuestions((prevQuestions) => {
            const updatedQuestions = [...prevQuestions];
            const question = updatedQuestions[questionIndex];

            if (question.questionType === 'choice' && question.answers.length > 2) {
                question.answers.splice(answerIndex, 1);
            }
            return updatedQuestions;
        });
    };

    const handleAnswerChange = (questionIndex, answerIndex, value) => {
        setQuestions((prevQuestions) => {
            const updatedQuestions = [...prevQuestions];
            const question = updatedQuestions[questionIndex];

            if (question.questionType === 'choice') {
                question.answers[answerIndex].text = value;
            }
            else if (question.questionType === 'contains' && answerIndex === 0) {
                question.answers[0].text = value;
            }
            return updatedQuestions;
        });
    };

    const handleCorrectChange = (questionIndex, answerIndex) => {
        setQuestions((prevQuestions) => {
            const updatedQuestions = [...prevQuestions];
            const question = updatedQuestions[questionIndex];

            if (question.questionType === "choice"){
                question.answers.forEach((a, idx) => {
                    a.isCorrect = idx === answerIndex;
                });
            }
            else if (question.questionType === "true or false") {
                question.answers.forEach((a, idx) => {
                    a.isCorrect = idx === answerIndex;
                });
            }
            

            return updatedQuestions;
        });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const hasCorrectAnswers = questions.every(q => q.answers.some(a => a.isCorrect));
        if (!hasCorrectAnswers) {
            setErrorMessages({ general: 'Každá otázka musí mít alespoň jednu správnou odpověď.' });
            return;
        }

        const formData = new FormData();
        formData.append('title', title);
        formData.append('description', description);

        if (quizImage) {
            formData.append('imageData', quizImage);
        } else {
            formData.append('imageData', null);
        }

        questions.forEach((q, index) => {
            formData.append(`questions[${index}].text`, q.text);
            formData.append(`questions[${index}].questionType`, q.questionType);
            formData.append(`questions[${index}].TimeForAnswer`, q.TimeForAnswer || 10);

            formData.append('quizOwner', user?.username || 'none');

            if (q.image) {
                formData.append(`questions[${index}].imageData`, q.image);
            }

            q.answers.forEach((a, answerIndex) => {
                formData.append(`questions[${index}].answers[${answerIndex}].text`, a.text);
                formData.append(`questions[${index}].answers[${answerIndex}].isCorrect`, a.isCorrect);
            });
        });

        try {
            const response = await fetch('https://localhost:7006/api/Quiz', {
                method: 'POST',
                body: formData,
                credentials: 'include',
            });

            if (response.ok) {
                navigate('/home');
            } else {
                const errorResponse = await response.json();
                setErrorMessages(errorResponse.errors);
            }
        } catch (error) {
            setErrorMessages({ general: 'Nastala chyba při odesílání kvízu.' });
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            {errorMessages && <div className="error-messages">{errorMessages.general}</div>}
    
            <div>
                <label>Title:</label>
                <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} required />
            </div>
    
            <div>
                <label>Description:</label>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} />
            </div>
    
            <div>
                <label>Quiz Image (png, jpg, jpeg):</label>
                <input type="file" accept="image/*" onChange={handleQuizImageChange} />
            </div>
    
            {questions.map((question, questionIndex) => (
                <div className="question-block" key={questionIndex}>
                    <label>Question {questionIndex + 1}:</label>
                    <input
                        type="text"
                        value={question.text}
                        onChange={(e) => handleQuestionChange(questionIndex, e.target.value)}
                        required
                    />
    
                    <div>
                        <label>Question Type:</label>
                        <select
                            value={question.questionType}
                            onChange={(e) => handleQuestionTypeChange(questionIndex, e.target.value)}
                            required
                        >
                            <option value="">Select Question Type</option>
                            <option value="choice">Výběr z variant</option>
                            <option value="true or false">Pravda nebo lež</option>
                            <option value="contains">Odhadnout slovo</option>
                        </select>
                    </div>
    
                    <div>
                        <label>Time for answer (seconds):</label>
                        <select
                            value={question.TimeForAnswer}
                            onChange={(e) => handleTimeForAnswerChange(questionIndex, e.target.value)}
                            required
                        >
                            <option value="10">10</option>
                            <option value="20">20</option>
                            <option value="30">30</option>
                            <option value="60">60</option>
                        </select>
                    </div>
    
                    <div>
                        <label>Question Image (optional):</label>
                        <input type="file" accept="image/*" onChange={(e) => handleImageChange(questionIndex, e)} />
                    </div>
    
                    <div>
                        <label>Answers:</label>
                        {question.answers.map((answer, answerIndex) => (
                            <div className="answer-block" key={answerIndex}>
                                <input
                                    type="text"
                                    value={answer.text}
                                    onChange={(e) => handleAnswerChange(questionIndex, answerIndex, e.target.value)}
                                    placeholder={`Answer ${answerIndex + 1}`}
                                />
                                <label>
                                    <input
                                        type="checkbox"
                                        checked={answer.isCorrect}
                                        onChange={() => handleCorrectChange(questionIndex, answerIndex)}
                                    />
                                    Correct
                                </label>
                                <button type="button" onClick={() => handleDeleteAnswer(questionIndex, answerIndex)}>Delete</button>
                            </div>
                        ))}
                        <button type="button" onClick={() => handleAddAnswer(questionIndex)}>Add Answer</button>
                    </div>
    
                    <button type="button" onClick={() => handleDeleteQuestion(questionIndex)}>Delete Question</button>
                </div>
            ))}
    
            <button type="button" onClick={handleAddQuestion}>Add Question</button>
            <button type="submit">Submit Quiz</button>
        </form>
    );
    
};

export default CreateQuiz;
