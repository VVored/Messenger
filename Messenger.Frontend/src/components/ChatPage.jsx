import React, { useEffect, useState } from 'react'
import axios from 'axios';
import ChatList from './ChatList';
import ChatMessages from './ChatMessages';
import CreateChat from './CreateChat';
import { HubConnectionBuilder } from "@microsoft/signalr";
import Profile from './Profile';



const ChatPage = () => {
    const [chats, setChats] = useState([]);
    const [createChatIsOpen, setCreateChatIsOpen] = useState(false);
    const [selectedChat, setSelectedChat] = useState(null);
    const [connection, setConnection] = useState(null);
    const [selectedUser, setSelectedUser] = useState(null);

    const StartConnection = async () => {
        const connection = new HubConnectionBuilder()
            .withUrl(`https://localhost:7192/chat?access_token=${localStorage.getItem('token')}`)
            .build();
        connection.start();
        setConnection(connection);
    }

    const JoinGroup = async (chatId) => {
        connection.invoke('JoinGroup', chatId + '');
    }

    const LeaveGroup = async (chatId) => {
        connection.invoke('LeaveGroup', chatId + '');
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
        LeaveGroup(selectedChat.chatId);
        setSelectedChat(null);
        setCreateChatIsOpen(true);
    }

    const openChatMessages = (chat) => {
        if (selectedChat) {
            LeaveGroup(selectedChat.chatId);
        }
        JoinGroup(chat.chatId);
        setSelectedChat(chat);
        setCreateChatIsOpen(false);
    }

    useEffect(() => {
        getUserChats().then((result) => {
            setChats(result.data);
        });
        StartConnection();
    }, [])

    return (
        <div style={{ display: "flex", height: '100vh', overflowY: 'hidden' }}>
            <ChatList chats={chats} openCreateChat={openCreateChat} openChatMessages={openChatMessages} />
            <div style={{ width: '100%', backgroundColor: "rgba(178, 178, 178, 0.5)" }}>
                {
                    createChatIsOpen ? <CreateChat setChats={setChats} setSelectedChat={setSelectedChat} setCreateChatIsOpen={setCreateChatIsOpen} /> : selectedChat ? <ChatMessages connection={connection} setSelectedUser={setSelectedUser} setConnection={setConnection} setChats={setChats} chat={selectedChat}></ChatMessages> : <h1 style={{ textAlign: "center" }}>Select a chat</h1>
                }
            </div>
            {
                selectedUser
                    ? <Profile selectedUser={selectedUser} setSelectedUser={setSelectedUser}></Profile>
                    : <div>

                    </div>
            }
        </div>
    )
}

export default ChatPage;