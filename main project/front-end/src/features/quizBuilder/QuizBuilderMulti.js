import React, { useEffect, useState } from 'react';
import { useParams } from "react-router-dom";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import defaultImage from '../quizBuilder/sign.jpg';
import AnswerCard from './AnswerCard';
import axios, { all } from 'axios';
import { useLocation } from "react-router-dom";
import { useNavigate } from 'react-router-dom';

const QuizBuilderMulti = () => {
    const { id } = useParams(); 
    const {lobbyId, setLobbyId} = useState(id);
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
    const [password, setPassword] = useState('');
    const [message, setMessage] = useState("");  
    const [isConnected, setIsConnected] = useState(false);
    const location = useLocation();
    const hasPassword = location.state?.hasPassword || false;
    const lobbyName = location.state?.lobbyName || "Neznámá lobby"; // Defaultní hodnota
    const [showPassword, setShowPassword] = useState(false);
    const [success, setSuccess] = useState(false);
    const [isPasswordCorrect, setIsPasswordCorrect]= useState(false);

    const [players, setPlayers] = useState([]); // Uloží hráče
    const [blockedPlayers, setBlockedPlayers] = useState([]);
   
    const [playerName, setPlayerName] = useState("");

    const [needTeam, setNeedTeam] = useState(false);
    const [teamNameInput, setTeamNameInput] = useState(""); // pro textové pole
    const [teams, setTeams] = useState([]); // seznam týmů
    const [teamAssignments, setTeamAssignments] = useState({}); // { [username]: teamName }

    const [changeButtons , setChangeButtons] = useState(false)

    const [serverMessage, setServerMessage] = useState("");

    const [teamMemberAnswer, setTeamMemberAnswer] = useState([])

    const [showResults, setShowResults] = useState(false);

    const [results, setResults] = useState([])

    const [usersAnswers, setUsersAnswers] = useState(false); // ukáže všem , kdo jak odpovědel na aktualní otázku , než se příjde další

    const [allUsersAnswers, setAllUsersAnswers] = useState([])

     const navigate = useNavigate();




    // Nastavení připojení k SignalR hubu
    useEffect(() => {
       
        
        const newConnection = new HubConnectionBuilder()
            .withUrl("https://localhost:7006/quiz-hub", {
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .build();

            console.log("aboba")

        newConnection.start()
            .then(() => {
                setConnection(newConnection);
                setMessage("Connected to SignalR hub.");
                console.log("Connected to SignalR hub");

                console.log("Lobby ID:", id);
            console.log("Heslo:", password);

                // Now you can invoke methods on the connection since it is established
                newConnection.invoke("JoinLobby", id, password, isModerator)
                    .then((success) => {
                        if (success) {
                            setMessage("Successfully joined lobby!");
                            setIsPasswordCorrect(true);
                            setIsConnected(true);
                            setLoginMassage('Úspěšně připojeno k lobby!');
                        } else {
                            setMessage('Neplatné heslo nebo lobby!');
                        }
                    })
                    .catch((error) => {
                        setMessage('Chyba při připojování k lobby');
                        console.error(error);
                    });
            })
            .catch((error) => {
                setMessage('Error connecting to the hub.');
                console.error(error);
            });

        // Handling incoming messages and setting state accordingly
        newConnection.on("ReceiveMessage", (data) => {
            console.log("Received data:", data);

            setUsersAnswers(false);
            setTeamMemberAnswer([]);


            if (data && typeof data === "string") {
                setReceivedData(data); 
        
                if (data.includes("has joined")) {
                    setSuccess(true);
                    console.log("hodnota succes : ");
                    console.log(success);
                }

                if(data.includes("ReceivePlayersList")){
                    console.log("---------------------------------")
                    console.log(data);
                }
            }

            if (data && data.id) {
                setQuestionId(data.id);
            }
            if (data && data.imageDataQuestion) {
                setQuizImage(`data:image/jpeg;base64,${data.imageDataQuestion}`);
            } else {
                setQuizImage(defaultImage); 
            }
            if (data && data.text) {
                setQuizText(data.text);
            }
            if (data && data.questionType) {
                setQuestionType(data.questionType);
            }
            if (data && data.answers) {
                setAnswer(prev => {
                    console.log("Received answers:", data.answers);
                    return data.answers;
                });
            }
        });


         //  Funkce pro odpojení uživatele při zavření stránky nebo změně karty
         const handleLeaveLobby = async (event) => {
            if (newConnection && newConnection.state === HubConnectionState.Connected) {
                try {
                    await newConnection.invoke("LeaveLobby", id);
                    console.log(` Uživatel opustil lobby: ${id}`);
                    newConnection.stop();
                } catch (error) {
                    console.error(" Chyba při odpojování od lobby:", error);
                }
            }
        };
    
        // Událost při obnově stránky 
        window.addEventListener("beforeunload", handleLeaveLobby);
    
        // Událost při stisknutí tlačítka zpět 
        window.addEventListener("popstate", handleLeaveLobby);
    
        return () => {
            // Odebrání listenerů při unmountování komponenty
            window.removeEventListener("beforeunload", handleLeaveLobby);
            window.removeEventListener("popstate", handleLeaveLobby);
        };
        
        
    }, [lobbyId, password, isModerator]);  

    useEffect(() => {
        console.log("Answer was updated:", answer);
    }, [answer]);

    useEffect(() => {
        checkIfModerator(id); 
       // logInToLobby(id);
    }, [id]); 

    useEffect(() => {
        if (connection) {
            connection.on("ReceivePlayersList", (playersData) => {
                console.log("Přijatý seznam hráčů:", playersData);
                setPlayers(playersData); // Uložení do state
            });
    
            
        }
    }, [connection]);

    useEffect(() => {
        if (connection){
            connection.on("ReceiveBlockedPlayersList", (blockedPlayersData) => {
                console.log("Zablokované", blockedPlayersData)
                setBlockedPlayers(blockedPlayersData)
            });
        }
    }, [connection])

    useEffect(() => {
        if (connection) {
            connection.on("TriggerPageReload", () => {
                
                window.location.reload();  // Obnoví stránku
            });
        }
    }, [connection]);
    
    useEffect(() => {
        if (connection) {
            connection.on("GetOutMessage", () => {
                
                window.location.reload();  // Obnoví stránku
            });
        }
    }, [connection]);

    // "TeamMemberAnswer"

    useEffect(() => {
        if (connection) {
            connection.on("CheckAnswer", (message) => {
                setServerMessage(message);
            });
        }
    
        return () => {
            if (connection) {
                connection.off("CheckAnswer");
            }
        };
    }, [connection]);

    useEffect(() => {
        if (connection){
            connection.on("ReceiveResults", (data) => {
                setShowResults(true);
                console.log("výsledky od backendu:", data);
                setResults(data);
                
            });
        }
    })

    

    useEffect(() => {
        if (connection) {
            connection.on("TeamMemberAnswer", (data) => {
                console.log("PŘIJATO OD BACKENDU:", data); 
    
                const { userName, answer } = data;
    
                setTeamMemberAnswer((prev) => {
                    const filtered = prev.filter(msg => msg.userName !== userName);
                    return [...filtered, { userName, answer }];
                });
            });
        }
    
        return () => {
            if (connection) {
                connection.off("TeamMemberAnswer");
            }
        };
    }, [connection]);

    useEffect(() => {
        if (connection) {
            connection.on("ShowUserAnswers", (data) => {
                setAllUsersAnswers([])
                setUsersAnswers(true);
                console.log("MEZIVÝSLEDKY od backendu:", data);
                setAllUsersAnswers(data)
            });
        }
    }, [connection]);     


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

    const gameStart = async (lobbyId) => {
        try {

            const hasTeam = Object.keys(teamAssignments).length > 0
            
            const requestData = hasTeam
            ? { teams: teamAssignments }
            : {}; // prázdné tělo, pokud nejsou týmy

            // {teams: {User: "soul", AkimAdmin: "aloloa"}}

            const response = await axios.post(
                `https://localhost:7006/api/Game/moderator-starts-game/${lobbyId}`,
                requestData,
                { withCredentials: true }
            );

            console.log(requestData)

            if (response.ok) {
                setGameStarted(true);
                
            } else {
                console.log("Failed to start game");
            }
        } catch (error) {
            console.error("Error starting game:", error);
        }
    };

    const deletePlayers = async () => {
        try {
             await fetch('https://localhost:7006/api/Lobby/delete/', {
                method: 'DELETE',
                credentials: 'include',
            });
        }
        catch (error) {
            console.error("Error starting game:", error);
        }
    }


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

            console.log("odeslaná data: ", requestData);
            console.log("Posílám lobbyId:", parsedLobbyId);

            await axios.post(`https://localhost:7006/api/Game/check-answer/${lobbyId}`, requestData, {
                withCredentials: true
            });

            console.log("jsou tymy ?", needTeam)
        } catch (error) {
            console.error('Error:', error);
        }
    };

    

    const HandleBlock = async (username, lobbyId) => {    
        try {
            const response = await axios.post(
                `https://localhost:7006/api/Lobby/block-player/${lobbyId}`,
                { username },  // Správný JSON formát
                { withCredentials: true }
            );
    
            console.log(` Hráč ${username} byl zablokován v lobby ${lobbyId}.`, response.data);
        } catch (error) {
            console.error(" Chyba při blokování hráče:", error.response?.data || error.message);
        }
    };

    const HandleUnblock = async (username, lobbyId) => {    
        try {
            const response = await axios.post(
                `https://localhost:7006/api/Lobby/unblock/${lobbyId}`,
                { username },  
                { withCredentials: true }
            );
    
            console.log(` Hráč ${username} byl razblokován v lobby ${lobbyId}.`, response.data);
        } catch (error) {
            console.error(" Chyba při razblokování hráče:", error.response?.data || error.message);
        }
    };

    const HandleGetOut = async (username, lobbyId) => {
        try {
            const response = await axios.delete(
                `https://localhost:7006/api/Lobby/get-out/${lobbyId}`,  
                {
                    data: { username }, 
                    withCredentials: true
                }
            );
            console.log(response.data); 
        } catch (error) {
            console.error("Chyba při blokování hráče:", error.response?.data || error.message);
        }
    };

    const createTeam = () => {
        if (!teamNameInput.trim()) return;
        setTeams(prev => [...prev, { teamName: teamNameInput, players: [] }]);
        setTeamNameInput("");
    };
    
    const toggleJoinTeam = (username, teamName) => {
        const currentTeam = teamAssignments[username];
    
        if (currentTeam === teamName) {
            // Odpoj hráče z týmu
            setTeamAssignments(prev => {
                const updated = { ...prev };
                delete updated[username];
                console.log("Rozdělení hráčů do týmů:", teamAssignments);
                return updated;
            });
        } else {
            // Připoj hráče do nového týmu
            setTeamAssignments(prev => ({
                ...prev,
                [username]: teamName
            }));
        }
    };

    const deleteTeam = (teamName) => {
        
        setTeams(prev => prev.filter(t => t.teamName !== teamName));
    
        
        setTeamAssignments(prev => {
            const updated = { ...prev };
            for (const [username, assignedTeam] of Object.entries(updated)) {
                if (assignedTeam === teamName) {
                    delete updated[username];
                }
            }
            return updated;
        });
    };
    
    const handleToLobbyMenu = () => {
        navigate('/lobby-menu');
    };

    

    return (
        <div>
            {showResults ? (
                <div>
                    <h2>Výsledky hry </h2>
                    {results.length > 0 ? (
                        <ul>
                            {results.map((player, index) => (
                                <li key={index}>
                                    {player.name}: {player.score} bodů
                                </li>
                            ))}
                        </ul>
                    ) : (
                        <p>Nikdo neodpověděl</p>
                    )}
                    {/* <button onClick={() => window.location.reload()}>Zpět do lobby</button> */}
                    <button onClick={handleToLobbyMenu}>Zpět do lobby</button>
                </div>
            ) : usersAnswers ? (
                <div>
                    {allUsersAnswers.length > 0 ? (
                        <ul>
                            {allUsersAnswers.map((player, index) => (
                                <li key={index}>
                                    {player.name}: {player.answer}
                                </li>
                            ))}
                        </ul>
                    ) : (
                        <p>Načítání výsledků...</p>
                    )}                    
                </div>
                
            ) :
            
            (
                <div>
                    <h1>Lobby Name: {lobbyName}</h1>

                    <h2>Lobby ID: {id}</h2>
                    <h2>Has Password: {hasPassword ? "Yes 🔒" : "No"}</h2>
                    <div>
                        <h3>Seznam hráčů:</h3>
                        {players.map((player, index) => (
                            <div key={index}>
                                <li>{player.username} – {player.score} bodů</li>
                                <button onClick={() => HandleBlock(player.username, id)}>Zablokovat</button>
                                <button onClick={() => HandleGetOut(player.username, id)}>Vyhodit z lobby</button>
                                
                                {needTeam && (
                                    <div>
                                        {teams.map((team, i) => (
                                            <div key={i} style={{ display: "inline-block", margin: "5px" }}>
                                            <span>{team.teamName}</span>
                                            <button onClick={() => toggleJoinTeam(player.username, team.teamName)}>
                                                {teamAssignments[player.username] === team.teamName ? "Leave team" : "Join to team"}
                                            </button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        ))}
                        <h3>Seznam zablokovaných hráčů:</h3>
                        <ul>
                            {blockedPlayers.map((blockedPlayer, index) => (
                                <div key={index}>
                                    <li>{blockedPlayer.userName}</li>
                                    <button onClick={() => HandleUnblock(blockedPlayer.userName, id)}>Razblokovat</button>
                                </div>
                            ))}
                        </ul>
                        
                    </div>
                    
                    <button onClick={() => deletePlayers()}>
                        Delete players
                    </button>
                    
                    {hasPassword && success === false &&(
                        <div>
                            <input
                            type={showPassword ? "text" : "password"}
                            placeholder="Password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            />
                            <label>
                                <span>Show password</span>
                                <input
                                    type="checkbox"
                                    checked={showPassword}
                                    onChange={() => setShowPassword(!showPassword)}
                                />
                            </label>
                        </div>
                    )}
                    
                    {loginMassage && <p style={{ color: "green", fontWeight: "bold" }}>{loginMassage}</p>}
                    
                    {isModerator && isPasswordCorrect && (
                        <div>
                            <button onClick={() => gameStart(id)}>
                                Moderator can start the game
                            </button>
                             
                            <label>
                                <span>Create Teams</span>
                                <input
                                    type='checkbox'
                                    checked={needTeam}
                                    onChange={() => setNeedTeam(!needTeam)}
                                />
                            </label>
                            
                            {needTeam && (
                                <div>
                                    <input
                                        type="text"
                                        value={teamNameInput}
                                        onChange={(e) => setTeamNameInput(e.target.value)}
                                        placeholder="Enter team name"
                                    />
                                    
                                    <button onClick={createTeam}>Create Team</button>
                                    
                                    <h4>Teams:</h4>
                                    <ul>
                                        {teams.map((team, index) => (
                                            <li key={index}>
                                                {team.teamName}
                                                <button onClick={() => deleteTeam(team.teamName)} style={{ marginLeft: "10px", color: "red" }}>
                                                    Smazat
                                                </button>
                                            </li>
                                        ))}
                                    </ul>

                                </div>
                            )}
                        </div>
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
                                <img src={quizImage} alt="Question" style={{ width: '400px', Maxheight: '300px' }} />
                            </div>
                        )}
                        
                        <div>
                            {serverMessage && <p>{serverMessage}</p>}
                        </div>

                        <div>
                            {teamMemberAnswer.map((msg,index) =>(
                            <p key={index}>
                                Hráč {msg.userName} hlasuje za variantu {msg.answer}
                            </p>
                        ))}
                        </div>
                        
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

            )}
            </div>
            
        );
};

export default QuizBuilderMulti;