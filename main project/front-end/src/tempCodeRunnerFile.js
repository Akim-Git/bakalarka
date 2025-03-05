import React, { useState } from 'react';

const type = [
    { id: 1, name: 'Typ A' },
    { id: 2, name: 'Typ B' },
    { id: 3, name: 'Typ C' },
    { id: 4, name: 'Typ D' }
];

const CreateQuiz = ({ onClick }) => {
    const [title, setTitle] = useState('');
    const [description, setDescription] = useState('');
    const [choosedType, setType] = useState([]);
    const [questions, setQuestions] = useState([{ text: '', answers: [{ text: '', isCorrect: false }] }]);
    const [image, setImage] = useState(null);
    const [errorMessages, setErrorMessages] = useState('');

    const handleAddQuestion = () => {
        setQuestions([...questions, { text: '', answers: [{ text: '', isCorrect: false }] }]);
    };

    const handleQuestionChange = (index, value) => {
        const updatedQuestions = [...questions];
        updatedQuestions[index].text = value;
        setQuestions(updatedQuestions);
    };

    const handleAddAnswer = (questionIndex) => {
        const updatedQuestions = [...questions];
        updatedQuestions[questionIndex].answers.push({ text: '', isCorrect: false });
        setQuestions(updatedQuestions);
    };

    const handleAnswerChange = (questionIndex, answerIndex, value) => {
        const updatedQuestions = [...questions];
        updatedQuestions[questionIndex].answers[answerIndex].text = value;
        setQuestions(updatedQuestions);
    };

    const handleCorrectChange = (questionIndex, answerIndex) => {
        const updatedQuestions = [...questions];
        const currentAnswer = updatedQuestions[questionIndex].answers[answerIndex];
        currentAnswer.isCorrect = !currentAnswer.isCorrect;
        setQuestions(updatedQuestions);
    };

    const handleImageChange = (e) => {
        setImage(e.target.files[0]);
    };


    const handleType = (e) => {
        const value = e.target.value;
        setChoosedType((prev) => {
            if (prev.includes(value)) {
                return prev.filter((type) => type !== value);
            } else {
                return [...prev, value];
            }
        });
    };
    
    

    

    const handleSubmit = async (e) => {
        e.preventDefault();
        const formData = new FormData();
        formData.append('title', title);
        formData.append('description', description);
    
        if (image) {
            formData.append('imageData', image);
        } else {
            formData.append('imageData', null);
        }
    
        questions.forEach((q, index) => {
            formData.append(`questions[${index}].text`, q.text);
    
            // Přidání quizType jako string pro každou otázku
            const selectedQuizType = choosedType.length > 0 ? choosedType[0] : null; // Zde si bereme první vybraný typ
            formData.append(`questions[${index}].quizType`, selectedQuizType || ""); // Pokud není vybrán, pošleme prázdný string
    
            q.answers.forEach((a, answerIndex) => {
                formData.append(`questions[${index}].answers[${answerIndex}].text`, a.text);
                formData.append(`questions[${index}].answers[${answerIndex}].isCorrect`, a.isCorrect);
            });
        });
    
        for (const [key, value] of formData.entries()) {
            console.log(`${key}:`, value);
        }
    
        try {
            const response = await fetch('https://localhost:7158/api/Quiz', {
                method: 'POST',
                body: formData,
            });
    
            if (response.ok) {
                const data = await response.json();
                console.log('Kvíz byl úspěšně vytvořen:', data);
                onClick();
                setErrorMessages('');
            } else {
                const errorResponse = await response.json();
                console.error('Chyba při vytváření kvízu:', errorResponse.errors);
                setErrorMessages(errorResponse.errors || { general: 'Nastala chyba při vytváření kvízu.' });
            }
        } catch (error) {
            console.error('Chyba při odesílání kvízu:', error);
            setErrorMessages({ general: 'Nastala chyba při odesílání kvízu.' });
        }
    };
    
    
    
    

    return (
        <form onSubmit={handleSubmit}>
            {errorMessages && (
                <div className="error-messages">
                    {errorMessages.general && <div className="error">{errorMessages.general}</div>}
                </div>
            )}
            <div>
                <label>Title:</label>
                <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} />
            </div>
            <div>
                <label>Description:</label>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} required />
            </div>
            {questions.map((question, questionIndex) => (
                <div key={questionIndex}>
                    <label>Question {questionIndex + 1}:</label>
                    <input
                        type="text"
                        value={question.text}
                        onChange={(e) => handleQuestionChange(questionIndex, e.target.value)}
                        required
                    />
                    <div>
                        {question.answers.map((answer, answerIndex) => (
                            <div key={answerIndex}>
                                <label>Answer {answerIndex + 1}:</label>
                                <input
                                    type="text"
                                    value={answer.text}
                                    onChange={(e) => handleAnswerChange(questionIndex, answerIndex, e.target.value)}
                                    required
                                />
                                <label>
                                    <input
                                        type="checkbox"
                                        checked={answer.isCorrect}
                                        onChange={() => handleCorrectChange(questionIndex, answerIndex)}
                                    />
                                    Correct
                                </label>
                            </div>
                        ))}
                    </div>
                    <button type="button" onClick={() => handleAddAnswer(questionIndex)}>Add Answer</button>
                </div>
            ))}
            <div>
                <label>Quiz Type:</label>
                {type.map((t) => (
                    <div key={t.id}>
                        <label>
                            <input
                                type="checkbox"
                                value={t.name}
                                checked={choosedType.includes(t.name)}
                                onChange={handleType}
                            />
                            {t.name}
                        </label>
                    </div>
                ))}
            </div>

            <div>
                <label>Image:</label>
                <input type="file" accept="image/*" onChange={handleImageChange} />
            </div>
            <button type="button" onClick={handleAddQuestion}>Add Question</button>
            <button type="submit">Create Quiz</button>
        </form>
    );
};

export default CreateQuiz;
