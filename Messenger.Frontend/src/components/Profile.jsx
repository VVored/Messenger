import React, { useEffect, useState } from 'react'
import { jwtDecode } from 'jwt-decode';
import { Link } from 'react-router-dom';

const Profile = ({ selectedUser, setSelectedUser, connection, chats, openChatMessages }) => {

    const [isSelectedUserAuthorizeNow, setIsSelectedUserAuthorizeNow] = useState(false);

    const createPrivateChat = async () => {
        connection.invoke('CreatePrivateChat', selectedUser.userId + '');
    }

    const getPrivateChat = () => {
        const token = localStorage.getItem('token');
        const decoced = jwtDecode(token);
        const userId = decoced["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
        const selectedUserId = selectedUser.userId;
        const privateChat = chats.find(chat => chat.chatType === 'private' && (chat.groupName === userId + ' ' + selectedUserId || chat.groupName === selectedUserId + ' ' + userId));
        return privateChat;
    }

    useEffect(() => {
        setIsSelectedUserAuthorizeNow(false);
        const token = localStorage.getItem('token');
        const decodedToken = jwtDecode(token);
        if (decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] === selectedUser.userId + '') {
            setIsSelectedUserAuthorizeNow(true);
        }
    }, [selectedUser])

    return (
        <div style={{ width: "30vw", overflowY: "auto", minHeight: "100vh" }}>
            <div style={{ backgroundColor: '#cccccc', height: '33%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', margin: '0' }}>
                <button onClick={() => { setSelectedUser(null) }} style={{ marginRight: 'auto', border: 'none', fontSize: '30px', color: 'white', backgroundColor: '#cccccc', cursor: 'pointer' }}>×</button>
                <img width='100px' height='100px' style={{ border: '0px solid black', borderRadius: '100%', objectFit: 'cover' }} src={`https://localhost:7192/api/files/${selectedUser.avatarUrl}`} alt={`${selectedUser.avatarUrl}`}></img>
                <h3 style={{ color: 'white' }}>{selectedUser.firstName} {selectedUser.lastName}</h3>
            </div>
            {
                isSelectedUserAuthorizeNow
                    ? <Link to={'/edit/' + selectedUser.userId}><button style={{ backgroundColor: 'rgba(178, 178, 178)', border: 'none', width: '100%', cursor: 'pointer', fontWeight: '700', color: 'white', padding: '2%' }}>Отредактировать профиль</button></Link>
                    : <button onClick={() => { const privateChat = getPrivateChat(); if (privateChat) { openChatMessages(privateChat) } else { createPrivateChat() } }} style={{ backgroundColor: 'rgba(178, 178, 178)', border: 'none', width: '100%', cursor: 'pointer', fontWeight: '700', color: 'white', padding: '2%' }}>Отправить сообщение</button>
            }
            <p>Имя пользователя: <b>@{`${selectedUser.username}`}</b></p>
        </div>
    )
}

export default Profile;