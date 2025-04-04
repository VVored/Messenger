import './App.css';
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import Login from './components/Login';
import PrivateRoute from './components/PrivateRoute';
import ChatPage from './components/ChatPage';
import RegisterPage from './components/RegisterPage';
import EditUser from './components/EditUser';

function App() {

  return (
    <div className="App">
      <BrowserRouter>
        <Routes>
          <Route path='login' element={<Login />}></Route>
          <Route path='register' element={<RegisterPage />}></Route>
          <Route path='*' element={<Navigate to="login" />}></Route>
          <Route element={<PrivateRoute />}>
            <Route path='/chats' element={<ChatPage />}></Route>
            <Route path='/edit'>
              <Route path=':id' element={<EditUser/>}></Route>
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </div>
  );
}

export default App;
