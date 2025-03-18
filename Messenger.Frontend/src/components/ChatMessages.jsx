import React, { useEffect, useId, useRef, useState } from 'react'
import axios from 'axios';
import Message from './Message';
import { HubConnectionBuilder } from "@microsoft/signalr"
import styles from './ChatMessages.module.css';
import styleChat from './Chat.module.css';
import leaveChatImg from '../imgs/leave_chat.png';

const ChatMessages = ({ chat, setChats }) => {

    const [messages, setMessages] = useState([]);
    const messagesEndRef = useRef(null);
    const [currentMessage, setCurrentMessage] = useState('');
    const [attachments, setAttachments] = useState([]);
    const [connection, setConnection] = useState(null);
    const [isJoined, setIsJoined] = useState(false);
    const [amountOfVh, setAmountOfVh] = useState(85);
    const [files, setFiles] = useState([]);

    const scrollToBottom = () => {
        messagesEndRef.current.scrollIntoView({ behaivor: 'smooth' });
    }

    const IsUserJoined = async () => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`https://localhost:7192/api/chats/${chat.chatId}/members/isjoined`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        setIsJoined(response.data);
    }

    const getChatMessages = async (chatId) => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`https://localhost:7192/api/messages/chats/${chatId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        return response;
    }

    const StartConnection = async (chatId) => {
        const connection = new HubConnectionBuilder()
            .withUrl(`https://localhost:7192/chat?access_token=${localStorage.getItem('token')}`)
            .build();
        connection.start().then(res => {
            connection.invoke("JoinGroup", chatId + '')
                .catch(err => {
                    console.log(err);
                });

        }).catch(err => {
            console.log(err);
        });
        connection.on('newMessage', message => {
            if (message.chatId === chat.chatId) {
                setMessages(prev => [...prev, message]);
            }
        });
        connection.on('userJoinChat', response => {
            console.log('userJoinedChat');
            setChats(prev => [...prev, chat]);
            setIsJoined(true);
        });
        connection.on('leaveChatMember', response => {
            console.log('leaveChatMember');
            setChats(prev => prev.filter(item => item !== chat));
            setIsJoined(false);
        })
        setConnection(connection);
    }

    const CloseConnection = () => {
        connection?.stop();
        setConnection(null);
    }

    const sendMessage = async (e, chatId, content) => {
        if (e.key === 'Enter') {
            uploadFiles(files);
            if (content !== '' || attachments.length > 0) {
                const token = localStorage.getItem('token');
                console.log(attachments);
                await fetch('https://localhost:7192/api/messages', {
                    method: 'POST',
                    body: JSON.stringify({ chatId, content, attachments }),
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
            }
        }
        setCurrentMessage('');
        setFiles([]);
        setAttachments([]);
    }

    const uploadFiles = async (files) => {
        let attachments = [];
        for (let i = 0; i < files.length; i++) {
            const formData = new FormData();
            formData.append('file', files[i]);
            const response = await axios.post('https://localhost:7192/api/files', formData);
            const attachment = {
                fileUrl: response.data,
                fileType: files[i].type,
                fileSize: files[i].size
            }
            attachments = [...attachments, attachment];
        }
        setAttachments(attachments);
    }

    const JoinChat = async () => {
        connection.invoke('JoinChat', chat.chatId + '');
    }

    const LeaveChat = async () => {
        connection.invoke('LeaveChat', chat.chatId + '');
    }

    useEffect(() => {
        IsUserJoined();
        setIsJoined(IsUserJoined());
        getChatMessages(chat.chatId).then(response => { setMessages(response.data) });
        CloseConnection();
        StartConnection(chat.chatId);
    }, [chat]);

    useEffect(scrollToBottom, [messages]);

    useEffect(() => {
        files.length > 0 ? setAmountOfVh(70) : setAmountOfVh(85);
        scrollToBottom();
    }, [files])

    return (
        <div style={{ minHeight: '100vh' }}>
            <div style={{ display: 'flex', height: '10vh', background: 'white' }}>
                <div style={{ display: 'flex', cursor: 'pointer', width: '85%', margin: 'auto 0' }} onClick={() => { console.log('1'); }}>
                    <img className={styleChat.chat_avatar} src={require(`../imgs/${chat.avatarUrl}`)} alt="" />
                    <p>{chat.groupName}</p>
                </div>
                <div style={{ display: 'flex', width: '15%' }}>
                    {isJoined ? <button className={styles.button} style={{ border: 'none' }} onClick={() => LeaveChat()}><img style={{ maxHeight: '50px' }} src={leaveChatImg} alt='' /></button> : <div></div>}
                </div>
            </div>
            <div style={{ height: amountOfVh + 'vh', overflowY: 'auto', }}>
                {
                    messages.map(message => {
                        return <Message key={message.messageId} message={message} />
                    })
                }
                <div ref={messagesEndRef} />
            </div>
            {
                files.length > 0
                    ? <div style={{ display: 'flex', height: '15vh', backgroundColor: 'white' }}>
                        {
                            Array.from(files).map(file => {
                                return <img key={file.name} src={URL.createObjectURL(file)}></img>
                            })
                        }
                    </div>
                    : <div>

                    </div>
            }
            <div style={{ height: '5vh', background: 'white', margin: 'auto', borderColor: 'rgba(178, 178, 178, 0.5)', borderWidth: '1px' }}>
                {
                    isJoined
                        ? <div style={{ display: 'flex', width: '100%', height: '100%' }}>
                            <div style={{ height: '111%', width: '5%' }}>
                                <label htmlFor="file" className={styles.file_label}></label>
                                <input type="file" id="file" className={styles.file_input} multiple onChange={e => { /*uploadFiles(e.target.files);*/ setFiles(e.target.files); }} />
                            </div>
                            <input className={styles.input} type="text" onChange={(e) => setCurrentMessage(e.target.value)} onKeyDown={(e) => sendMessage(e, chat.chatId, currentMessage, attachments)} placeholder="Text your message" />
                        </div>
                        : <button className={styles.button} onClick={() => { JoinChat() }}>Join chat</button>}
            </div>
        </div>
    )
}

export default ChatMessages;