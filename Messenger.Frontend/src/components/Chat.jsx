import React from 'react'
import style from './Chat.module.css';

const Chat = ({chat, onClick}) => {
    return (
        <div className={style.chat_container} onClick={onClick}>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
        </div>
    )
}

export default Chat;