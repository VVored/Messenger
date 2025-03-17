import React, { useState } from 'react';
import style from './Login.module.css'
import axios from 'axios';
import FilePicker from './FilePicker';

const CreateChat = ({setChats, setSelectedChat, setCreateChatIsOpen}) => {
    const [chatType, setChatType] = useState('public');
    const [groupName, setGroupName] = useState('');
    const [description, setDescription] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('a0119854-3009-4e46-8029-4237a3b2a220.jpg');
    const [error, setError] = useState('');

    const createGroupChat = async () => {
        console.log(groupName, description);
        if (groupName !== '' || description !== '') {
            const token = localStorage.getItem('token');
            try {
                const response = await axios.post('https://localhost:7192/api/chats', {
                    chatType: chatType,
                    groupName: groupName,
                    description: description,
                    avatarUrl: avatarUrl
                }, {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    }
                });
                console.log(response);
                setCreateChatIsOpen(null);
                setChats(prev => [...prev, response.data]);
                setSelectedChat(response.data);
            } catch (e) {
                setError(e);
                console.log('ne good');
            }
        }
    }

    return (
        <div className={style.container}>
            <FilePicker setAvatarUrl={setAvatarUrl}/>
            <input className={style.input} type="text" value={groupName} onChange={(e) => setGroupName(e.target.value)} placeholder="Group chat name" />
            <input className={style.input} type="text" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Descrtiption" />
            <button className={style.button} onClick={() => createGroupChat()}>Create chat</button>
            {error ?? <p className={style.error}>{error}</p>}
        </div>
    );
};

export default CreateChat;