import React from 'react'
import Chat from './Chat';

const ChatList = ({ chats }) => {
    return (
        <div style={{ width: "30vw" }}>
            {
                chats.map(chat =>
                    <Chat key={chat.chatId} chat={chat}></Chat>
                )
            }
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
            <img className={style.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)}></img>
            <div>
                <p>{chat.groupName}</p>
            </div>
        </div>
    )
}

export default ChatList;