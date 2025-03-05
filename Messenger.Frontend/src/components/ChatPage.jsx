import React, { useEffect, useMemo, useRef, useState } from 'react'
import axios from 'axios';
import ChatList from './ChatList';
import ChatMessages from './ChatMessages';
import CreateChat from './CreateChat';



const ChatPage = () => {
    const [chats, setChats] = useState([]);
    const [createChatIsOpen, setCreateChatIsOpen] = useState(false);
    const [selectedChat, setSelectedChat] = useState(null);
    const lastSelectedChat = usePrevious(selectedChat);

    function usePrevious(value) {
        const ref = useRef();
        useEffect(() => {
            ref.current = value;
        });
        return ref.current;
    }

    const getUserChats = async () => {
        const token = localStorage.getItem('token');
        const response = await axios.get('https://localhost:7192/api/chats/my', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        return response;
    }

    const openCreateChat = () => {
        setSelectedChat(null);
        setCreateChatIsOpen(true);
    }

    const openChatMessages = (chat) => {
        setSelectedChat(chat);
        setCreateChatIsOpen(false);
    }

    useEffect(() => {
        getUserChats().then((result) => {
            setChats(result.data);
        });
    }, [])

    return (
        <div style={{ display: "flex", height: '100vh', overflowY: 'hidden' }}>
            <ChatList chats={chats} openCreateChat={openCreateChat} openChatMessages={openChatMessages} />
            <div style={{ width: "70vw", backgroundColor: "rgba(178, 178, 178, 0.5)" }}>
                {
                    createChatIsOpen ? <CreateChat /> : selectedChat ? <ChatMessages chats={chats} chat={selectedChat}></ChatMessages> : <h1 style={{ textAlign: "center" }}>Select a chat</h1>
                }
            </div>
        </div>
    )
}

export default ChatPage;