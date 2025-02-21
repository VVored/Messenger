import React from 'react'
import Chat from './Chat';

const ChatList = ({ chats, selectedChat, setSelectedChat }) => {
    return (
        <div style={{ width: "30vw", overflowY: "auto" }}>
            {
                chats.map(chat =>
                    <Chat key={chat.chatId} chat={chat} onClick={() => setSelectedChat(chat)}></Chat>
                )
            }
        </div>
    )
}

export default ChatList;