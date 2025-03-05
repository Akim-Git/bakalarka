import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import defaultImage from '../home/default-quiz-image.jpg';
import axios from 'axios';
import QuizCard from './QuizCard';
import './HomeStyle.css';

const Home = () => {
    const navigate = useNavigate();
    const { logout } = useAuth();
    const [quizzes, setQuizzes] = useState([]);
    const [isAdmin, setIsAdmin] = useState(false);
    const [skip, setSkip] = useState(0);
    const [showModal, setShowModal] = useState(false);
    const [isCreateLobby, setIsCreateLobby] = useState(false);
    const [lobbyName, setLobbyName] = useState('');
    const [quizToLobby, setQuizToLobby] = useState('');
    const [hasLobby, setHasLobby] = useState(false);
    const [showPasswordField, setShowPasswordField] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [lobbyPassword, setLobbyPassword] = useState("")
    const [isLoading, setIsLoading] = useState(false);

    const fetchQuizzes = async (append = false) => {
        try {
            const response = await fetch(`https://localhost:7006/api/Home/get-quiz-preview?skip=${skip}`, {
                method: 'GET',
                credentials: 'include',
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const { quizzes: newQuizzes, isAdmin } = await response.json();
            setIsAdmin(isAdmin);

            if (append) {
                setQuizzes((prevQuizzes) => [...prevQuizzes, ...newQuizzes]);
            } else {
                setQuizzes(newQuizzes);
            }
        } catch (error) {
            console.error('Error fetching quizzes:', error);
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm('Chceš tohle odstranit, ty admine jeden?')) {
            try {
                const response = await fetch(`https://localhost:7006/api/Home/${id}`, {
                    method: 'DELETE',
                    credentials: 'include',
                });

                if (!response.ok) {
                    throw new Error('Failed delete');
                }

                setQuizzes((prevQuizzes) => prevQuizzes.filter((quiz) => quiz.id !== id));
            } catch (error) {
                console.error('Error deleting quiz:', error);
            }
        }
    };

    const handleLobbyCreate = async () => {
        setIsLoading(true);
        try {
            

            const requestData = { 
                name: lobbyName,
                quizId: quizToLobby,
                lobbyOwner: "",
                isActive: true,
                lobbyPassword: lobbyPassword  
            };

            console.log("Odesílám tato data:", {
                lobbyName,
                quizToLobby,
                lobbyPassword
                
            });

            const response = await axios.post(
                'https://localhost:7006/api/Lobby/create-lobby',
                requestData,
                { withCredentials: true }
            );

            console.log('Lobby response:', response.data);
        } catch (error) {
            console.error('Lobby error:', error);
            if (error.response && error.response.status === 409) {
                alert('Lobby with this name already exists. Please choose a different name.');
                window.location.reload();
            } else {
                alert('Lobby failed: ' + (error.response ? error.response.data.message : error.message));
                window.location.reload();
            }
        } finally {
            setHasLobby(false);
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchQuizzes();
    }, [skip]);

    const loadMoreQuizzes = () => {
        setSkip((prevSkip) => prevSkip + 9);
        fetchQuizzes(true);
    };

    const handleCreateQuiz = () => {
        navigate('/create-quiz');
    };

    const handleLogout = async () => {
        await logout();
        navigate('/login');
    };

    const handleNavigate = (id) => {
        navigate(`/quiz-builder/${id}`);
    };

    const handleNavigateLobbyMenu = () => {
        navigate('/lobby-menu')
    }

    const getImageType = (base64String) => {
        if (base64String.startsWith('/9j/')) return 'jpeg';
        if (base64String.startsWith('iVBORw0KGgo')) return 'png';
        return 'jpeg';
    };

    const handleOpenModal = () => {
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setIsCreateLobby(false);
    };

    const handleCreateLobby = () => {
        setIsCreateLobby(true);
    };

    const handleLobbyName = () => {
        setShowModal(false);
        setHasLobby(true);
        handleCloseModal();
    };

    const handleQuizToLobby = (id, title) => {
        const confirmation = window.confirm(`Chcete vybrat tento kvíz: "${title}" pro lobby?`);
        if (confirmation) {
            setQuizToLobby(id);
            
        }
    };

    useEffect(() => {
        
        if (quizToLobby) {
            handleLobbyCreate();
        }
    }, [quizToLobby]);

    return (
        <div>
            <h2>Úvodní</h2>
            <button onClick={handleCreateQuiz}>Create Quiz</button>
            <button onClick={handleLogout}>Logout</button>
            <button onClick={handleOpenModal}>Try Multiplayer</button>

            <div className="quiz-grid">
                {quizzes.map((quiz) => {
                    const image = quiz.image && quiz.image !== 'null'
                        ? `data:image/${getImageType(quiz.image)};base64,${quiz.image}`
                        : defaultImage;

                    return (
                        <QuizCard
                            key={quiz.id}
                            title={quiz.title}
                            id={quiz.id}
                            description={quiz.description}
                            image={image}
                            isAdmin={isAdmin}
                            hasLobby={hasLobby}
                            onDelete={handleDelete}
                            onQuizToLobby={handleQuizToLobby}
                            onNavigate={handleNavigate}
                        />
                    );
                })}
            </div>

            <button onClick={loadMoreQuizzes}>Další kvízy</button>

            {isLoading && (
                <div className="loading-indicator">
                    <span>Loading...</span>
                </div>
            )}

            {showModal && (
                <div className="modal-overlay">
                    <div className="modal">
                        {!isCreateLobby ? (
                            <>
                                <h3>Multiplayer</h3>
                                <div className="modal-options">
                                    <button onClick={handleNavigateLobbyMenu}>Find Lobby</button>
                                    <button onClick={handleCreateLobby}>Create Lobby</button>
                                </div>
                            </>
                        ) : (
                            <>
                                <h3>Create Lobby</h3>
                                <input
                                    type="text"
                                    placeholder="Enter Lobby Name"
                                    value={lobbyName}
                                    onChange={(e) => setLobbyName(e.target.value)}
                                />

                                {lobbyName ? (
                                    <>
                                        <button onClick={handleLobbyName}>Výbrat kvíz</button>
                                        <span>Create lobby password</span>
                                        <input
                                            type='checkbox'
                                            checked={showPasswordField}
                                            onChange={() => setShowPasswordField(!showPasswordField)}
                                        />
                                    
                                    </>
                                ) : null}

                                {showPasswordField ? (
                                    <>
                                        <input
                                            type={showPassword ? "text" : "password"}
                                            placeholder="Password"
                                            value={lobbyPassword}
                                            onChange={(e) => setLobbyPassword(e.target.value)}
                                            required
                                        />

                                        <input
                                            type='checkbox'
                                            checked={showPassword}
                                            onChange={()=> setShowPassword(!showPassword)}
                                        />
                                    </>
                                ): null}
                                
                            </>
                        )}
                        <button onClick={handleCloseModal}>Close</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Home;
