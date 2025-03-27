import React, { useEffect, useState } from 'react'
import style from './Chat.module.css';
import { jwtDecode } from 'jwt-decode';

const Chat = ({ chat, onClick }) => {

    const [privateChatAvatarUrl, setPrivateChatAvatarUrl] = useState('');
    const [privateChatName, setPrivateChatName] = useState('');

    useEffect(() => {
        setPrivateChatAvatarUrl('');
        setPrivateChatName('');
        const token = localStorage.getItem('token');
        const decoded = jwtDecode(token);
        chat.chatMembers.forEach(chatMember => {
            if (chatMember.user.userId + '' !== decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]) {
                setPrivateChatAvatarUrl(chatMember.user.avatarUrl);
                setPrivateChatName(chatMember.user.username);
            }
        });
    }, [chat])

    return (
        <div className={style.chat_container} onClick={onClick}>
            {
                chat.chatType === 'public'
                    ? <img className={style.chat_avatar} src={`https://localhost:7192/api/files/${chat.avatarUrl}`} alt='img' />
                    : <img className={style.chat_avatar} src={`https://localhost:7192/api/files/${privateChatAvatarUrl}`} alt='img' />
            }
            {
                chat.chatType === 'public'
                    ? <p>{chat.groupName}</p>
                    : <p>{privateChatName}</p>
            }
        </div >
    )
}

export default Chat;