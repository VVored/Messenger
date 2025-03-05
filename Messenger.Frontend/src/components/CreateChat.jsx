import React, { useState } from 'react';
import style from './Login.module.css'

const CreateChat = ({}) => {
    const [chatType, setChatType] = useState('public');
    const [groupName, setGroupName] = useState('');
    const [description, setDescription] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('1910995.png');
    const [error, setError] = useState('');

    const createGroupChat = async () => {
        console.log(groupName, description);
        if (groupName !== '' || description !== '') {
            const token = localStorage.getItem('token');
            try {
                const response = await fetch('https://localhost:7192/api/chats', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json', 
                        'Authorization': `Bearer ${token}`
                    },
                    body: JSON.stringify({chatType, groupName, description, avatarUrl})
                });
                console.log(response);
            } catch (e) {
                setError(e);
                console.log('ne good');
            }
        }
    }

    return (
        <div className={style.container}>
            <input className={style.input} type="text" value={groupName} onChange={(e) => setGroupName(e.target.value)} placeholder="Group chat name" />
            <input className={style.input} type="text" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Descrtiption" />
            <button className={style.button} onClick={() => createGroupChat()}>Create chat</button>
            {error ?? <p className={style.error}>{error}</p>}
        </div>
    );
};

export default CreateChat;