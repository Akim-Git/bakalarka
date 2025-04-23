import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import './LobbyMenuStyle.css';

const LobbyMenu = () => {
    const [lobbies, setLobbies] = useState([]);
    const [searchTerm, setSearchTerm] = useState("");
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

    const handleNavigateToLobby = async (id, hasPassword, name) => {
        try {
            const response = await axios.get('https://localhost:7006/api/Lobby/does-lobby-exists', {
                params: { lobbyId: id },
                withCredentials: true
            });
    
            if (response.status === 200) {
                navigate(`/quiz-builder-multiplayer/${id}`, { state: { hasPassword, lobbyName: name } });
                console.log(`Navigace do quiz-builder-multiplayer s ID "${id}", hasPassword: ${hasPassword}, lobbyName: ${name}`);
            }
        } catch (error) {
            console.error('Error deleting quiz:', error);
            alert("Toto lobby už neexistuje nebo bylo smazáno. Stránka se nyní obnoví.");
            window.location.reload();
        }
    };
    
    const filteredLobbies = lobbies.filter(lobby =>
        lobby.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <div>
            <input
                type="text"
                placeholder="Hledat lobby..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
            />
            
            <div className="lobby-grid">
                {filteredLobbies.map((lobby) => (
                    <div key={lobby.id} className="lobby-item">
                        <p><strong>Lobby Name:</strong> {lobby.name} {lobby.hasPassword && "🔒"}</p>
                        <button onClick={() => handleNavigateToLobby(lobby.id, lobby.hasPassword, lobby.name)}>Join</button>
                    </div>
                ))}
            </div>
            
        </div>
    );
};

export default LobbyMenu;