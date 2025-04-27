import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const Profile = () => {
    const [userData, setUserData] = useState(null);
    const navigate = useNavigate();
    const [showQuizzes, setShowQuizzes] = useState(false);
    const [quizzes, setQuizzes] = useState([]);
    const [showModal, setShowModal] = useState(false);
    const [lobbyName, setLobbyName] = useState('');
    const [quizToLobby, setQuizToLobby] = useState('');
    const [showPasswordField, setShowPasswordField] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [lobbyPassword, setLobbyPassword] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        axios.get('https://localhost:7006/api/Profile/get-profile-data', {
            withCredentials: true
        })
        .then(response => {
            setUserData(response.data);
        })
        .catch(error => {
            console.error('Error loading profile:', error);
            if (error.response && error.response.status === 401) {
                navigate('/login');
            }
        });
    }, [navigate]);

    const handleShowMyQuizies = async () => {
        const newShowState = !showQuizzes;
        setShowQuizzes(newShowState);

        if (newShowState) {
            try {
                const response = await axios.get(
                    'https://localhost:7006/api/Profile/get-quizzes',
                    { withCredentials: true }
                );
                setQuizzes(response.data);
            } catch (error) {
                console.error('Error getting quizzes:', error);
            }
        }
    };

    const handleOpenModal = (quizId) => {
        setQuizToLobby(quizId);
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setLobbyName('');
        setQuizToLobby('');
        setLobbyPassword('');
        setShowPasswordField(false);
        setShowPassword(false);
    };

    const handleLobbyCreate = async () => {
        if (!lobbyName || !quizToLobby) {
            alert('Vyplňte název lobby a vyberte kvíz!');
            return;
        }

        setIsLoading(true);
        try {
            const requestData = {
                name: lobbyName,
                quizId: quizToLobby,
                lobbyOwner: "", 
                lobbyPassword: lobbyPassword
            };

            console.log("Odesílám lobby:", requestData);

            const response = await axios.post(
                'https://localhost:7006/api/Lobby/create-lobby',
                requestData,
                { withCredentials: true }
            );

            console.log('Lobby vytvořeno:', response.data);
            alert('Lobby úspěšně vytvořeno!');
            handleCloseModal();
        } catch (error) {
            console.error('Chyba při vytváření lobby:', error);
            if (error.response && error.response.status === 409) {
                alert('Lobby se stejným názvem již existuje.');
            } else {
                alert('Chyba při vytváření lobby.');
            }
        } finally {
            setIsLoading(false);
        }
    };

    if (!userData) {
        return <div>Loading...</div>;
    }

    return (
        <div>
            <h1>Uživatelské jméno: {userData.username}</h1>

            <button onClick={handleShowMyQuizies}>
                {showQuizzes ? 'Skrýt seznam kvízů' : 'Zobrazit seznam kvízů'}
            </button>

            {showQuizzes && (
                <div>
                    <h2>Seznam kvízů:</h2>
                    {quizzes.length > 0 ? (
                        <ul>
                            {quizzes.map((quiz, index) => (
                                <div key={index}>
                                    <li>{quiz.quizName}</li>
                                    <button onClick={() => handleOpenModal(quiz.quizId)}>
                                        Vytvořit lobby
                                    </button>
                                </div>
                            ))}
                        </ul>
                    ) : (
                        <p>Nemáte žádné kvízy.</p>
                    )}
                </div>
            )}

            {showModal && (
                <div className="modal-overlay">
                    <div className="modal">
                        <h3>Vytvořit novou lobby</h3>

                        <input
                            type="text"
                            placeholder="Zadejte název lobby"
                            value={lobbyName}
                            onChange={(e) => setLobbyName(e.target.value)}
                        />

                        <div style={{ marginTop: '10px' }}>
                            <label>
                                <input
                                    type="checkbox"
                                    checked={showPasswordField}
                                    onChange={() => setShowPasswordField(!showPasswordField)}
                                />
                                Přidat heslo k lobby
                            </label>
                        </div>

                        {showPasswordField && (
                            <div>
                                <input
                                    type={showPassword ? "text" : "password"}
                                    placeholder="Heslo k lobby"
                                    value={lobbyPassword}
                                    onChange={(e) => setLobbyPassword(e.target.value)}
                                />
                                <div>
                                    <label>
                                        <input
                                            type="checkbox"
                                            checked={showPassword}
                                            onChange={() => setShowPassword(!showPassword)}
                                        />
                                        Zobrazit heslo
                                    </label>
                                </div>
                            </div>
                        )}

                        <div style={{ marginTop: '20px' }}>
                            <button onClick={handleLobbyCreate} disabled={isLoading}>
                                {isLoading ? 'Vytváření lobby...' : 'Vytvořit'}
                            </button>
                            <button onClick={handleCloseModal} style={{ marginLeft: '10px' }}>
                                Zavřít
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Profile;
