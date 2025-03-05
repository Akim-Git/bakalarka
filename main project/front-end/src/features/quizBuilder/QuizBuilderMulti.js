import React, { useEffect, useState } from 'react';
import { useParams } from "react-router-dom";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import defaultImage from '../quizBuilder/sign.jpg';
import AnswerCard from './AnswerCard';
import axios from 'axios';

const QuizBuilderMulti = () => {
    const { id } = useParams(); 
    const [connection, setConnection] = useState(null);
    const [isModerator, setIsModerator] = useState(false);
    const [gameStarted, setGameStarted] = useState(false);
    const [userInput, setUserInput] = useState("");
    const [receivedData, setReceivedData] = useState(""); // Ukládáme příchozí data jako text
    const [quizText, setQuizText] = useState(""); 
    const [quizImage, setQuizImage] = useState(""); 
    const [questionType, setQuestionType] = useState("");
    const [questionId, setQuestionId] = useState("");
    const [answer, setAnswer] = useState([]);
    const [loginMassage, setLoginMassage] = useState("");
    const [receivedResults, setReceivedResults] = useState([]);


    // Nastavení připojení k SignalR hubu
    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl("https://localhost:7006/quiz-hub", {
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .build();

        newConnection.start()
            .then(() => {
                console.log("Connected to SignalR hub");
                setConnection(newConnection);
                newConnection.invoke("JoinLobby", id)
                    .then(() => {
                        checkIfModerator(id);
                        console.log(`Joined lobby: ${id}`);
                        
                        logInToLobby(id);
                    })
                    .catch((error) => {
                        console.error("Error joining lobby:", error);
                    });
            })
            .catch((error) => {
                console.error("Error establishing connection:", error);
            });

        // Posloucháme zprávy od hubu a přímo je zobrazujeme jako text
        newConnection.on("ReceiveMessage", (data) => {
            console.log("Received data:", data);

            // Příchozí data převedeme na text a zobrazíme
            setReceivedData(JSON.stringify(data, null, 2)); // Převod na čitelný formát JSON

            if (data && data.id){
                setQuestionId(data.id);
            }

            // Pokud data obsahují obrázek, uložíme ho
            if (data && data.imageDataQuestion) {
                setQuizImage(`data:image/jpeg;base64,${data.imageDataQuestion}`);
            } else {
                setQuizImage(defaultImage); 
            }
           
            if (data && data.text) {
                setQuizText(data.text);
            }

            if (data && data.questionType){
                setQuestionType(data.questionType);
            }

            if (data && data.answers) {
                setAnswer(prev => {
                    console.log("AAAAAAAAAAAAA", data.answers);
                    console.log("bbbbbbbbbb", answer);
                    return data.answers;
                });
            }
            
        });

        newConnection.on("ReceiveResults", (data) => {
            console.log("Obdržené výsledky:", data);
            setReceivedResults(data); 
        });
        

        return () => {
            if (newConnection && newConnection.state === HubConnectionState.Connected) {
                newConnection.invoke("LeaveLobby", id)
                    .then(() => {
                        console.log(`Left lobby: ${id}`);
                        newConnection.stop();
                    });
            }
        };
    }, [id]);

    useEffect(() => {
        console.log("Answer was updated:", answer);
    }, [answer]);

    // Kontrola jestli je uživatel moderátor
    const checkIfModerator = async (lobbyId) => {
        try {
            const response = await fetch(`https://localhost:7006/api/Lobby/is-moderator/${lobbyId}`, {
                method: 'GET',
                credentials: 'include',
            });
            if (response.ok) {
                const data = await response.json();
                setIsModerator(!!data.sendModerator);
            }
        } catch (error) {
            console.error("Error checking moderator status:", error);
        }
    };

    // Spuštění hry
    const gameStart = async (lobbyId) => {
        try {
            const response = await fetch(`https://localhost:7006/api/Game/moderator-starts-game/${lobbyId}`, {
                method: 'GET',
                credentials: 'include',
            });

            if (response.ok) {
                setGameStarted(true);
            } else {
                console.log("Failed to start game");
            }
        } catch (error) {
            console.error("Error starting game:", error);
        }
    };

    const logInToLobby = async (lobbyId) => {
        try {
            const response = await fetch(`https://localhost:7006/api/Lobby/log-in-lobby/${lobbyId}`, {
                method: 'POST',
                credentials: 'include',
            });
    
            if (!response.ok) {
                setLoginMassage("máš smůlu, nevýšlo se přihlasit do lobby");
                throw new Error('Nepodařilo se přihlásit do lobby');
            }
    
            console.log(" Úspěšně přihlášen do lobby");
            setLoginMassage("úspěšmé jste se přihůásil do lobby");
        } catch (error) {
            console.error(error);
        }
    };

    const sendAnswer = async (lobbyId, selectedAnswer) => {
        try {
            const parsedLobbyId = Number(lobbyId); 
            if (isNaN(parsedLobbyId)) {
                console.error("Neplatné lobbyId:", lobbyId);
                return;
            }

            const requestData = {
                questionId,
                userInput: questionType === "contains" ? userInput : selectedAnswer, 
                questionType,
            };

            console.log("odesláná data: ", requestData);
            console.log("Posílám lobbyId:", parsedLobbyId);

            await axios.post(`https://localhost:7006/api/Game/check-answer/${lobbyId}`, requestData, {
                withCredentials: true
            });
        } catch (error) {
            console.error('Error:', error);
        }
    };

    return (
        <div>
            <h1>Lobby ID: {id}</h1>

            {loginMassage && <p style={{ color: "green", fontWeight: "bold" }}>{loginMassage}</p>}
            
            {/* Tlačítko pro moderátora ke spuštění hry */}
            {isModerator && (
                <button onClick={() => gameStart(id)}>
                    Moderator can start the game
                </button>
            )}

            <div>
                <h3>Received Data from Hub:</h3>
                <pre style={{ backgroundColor: '#f4f4f4', padding: '10px' }}>
                    {receivedData ? receivedData : "Waiting for data..."}
                </pre>

                {receivedResults.length > 0 && (
                    <div>
                        <h3>Výsledky hry:</h3>
                        <ul>
                            {receivedResults.map((player, index) => (
                                <li key={index}>{player.username}: {player.score} bodů</li>
                            ))}
                        </ul>
                    </div>
                )}


                {quizImage && (
                    <div>
                        <h4>Image:</h4>
                        <img src={quizImage} alt="Question" style={{ maxWidth: '100%', height: 'auto' }} />
                    </div>
                )}

                {quizText && (
                    <div>
                        <h3>Question:</h3>
                        <p>{quizText}</p>
                    </div>
                )}

                {questionType && (
                    <div>
                        <p>question Type {questionType}</p>
                    </div>
                )}                 
                
                <div>
                    {questionType === "contains" ? (
                        <AnswerCard
                            type="contains"
                            userInput={userInput}
                            onInputChange={(e) => setUserInput(e.target.value)}
                            onAnswer={() => sendAnswer(id, userInput)}
                            //onAnswer={() => console.log("Vybraná odpověď:", userInput)}
                        />
                    ) : (
                        <div>
                            {answer.map((ans, index) => (
                                <AnswerCard
                                    key={index}
                                    type={questionType}
                                    answer={ans}

                                  //  onAnswer={() => console.log("Vybraná odpověď:", ans)}

                                    //onAnswer={sendAnswer}

                                    onAnswer={() => sendAnswer(id, ans)}
                                />
                                ))}
                    </div>
                    )}
                    </div>


               
                
            </div>
        </div>
    );
};

export default QuizBuilderMulti;
