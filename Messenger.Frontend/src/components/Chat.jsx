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
                    ? <img className={style.chat_avatar} src={`http://45.144.222.67:5266/api/files/${chat.avatarUrl}`} alt='img' />
                    : <img className={style.chat_avatar} src={`http://45.144.222.67:5266/api/files/${privateChatAvatarUrl}`} alt='img' />
            }
            <div style={{width: '100%'}}>
                {
                    chat.chatType === 'public'
                        ? <p style={{fontWeight: '700'}}>{chat.groupName}</p>
                        : <p style={{fontWeight: '700'}}>{privateChatName}</p>
                }
                {
                    chat.lastMessage
                        ? <p style={{fontSize: '12px'}}>{chat.lastMessage.sender.firstName} {chat.lastMessage.sender.lastName}: {chat.lastMessage.content}</p>
                        : <p></p>
                }
            </div>

        </div >
    )
}

export default Chat;