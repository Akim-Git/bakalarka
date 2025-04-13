import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import './LobbyMenuStyle.css';

const LobbyMenu = () => {
    const [lobbies, setLobbies] = useState([]);
    const navigate = useNavigate();

    const fetchLobbies = async () => {
        try {
            const response = await axios.get('https://localhost:7006/api/Lobby/get-lobbies', {
                withCredentials: true
            });
            setLobbies(response.data.lobbies);
        } catch (error) {
            console.error('Chyba v obdržení dat:', error);
        }
    };

    useEffect(() => {
        fetchLobbies();
    }, []);

    const handleNavigateToLobby = (id, hasPassword, name) => {
        navigate(`/quiz-builder-multiplayer/${id}`, { state: { hasPassword , lobbyName: name } });
        console.log(`Navigace do quiz-builder-multiplayer s ID "${id}" a hasPassword: ${hasPassword}, lobbyName: ${name}`);
    };

    return (
        <div className="lobby-grid">
            {lobbies.map((lobby) => (
                <div key={lobby.id} className="lobby-item">
                    <p><strong>Lobby Name:</strong> {lobby.name} {lobby.hasPassword && "🔒"}</p>
                    <button onClick={() => handleNavigateToLobby(lobby.id, lobby.hasPassword, lobby.name)}>Join</button>
                </div>
            ))}
        </div>
    );
};

export default LobbyMenu;
