import './App.css';
import {BrowserRouter, Navigate, Route, Routes} from 'react-router-dom'
import Login from './components/Login';
import PrivateRoute from './components/PrivateRoute';
import ChatPage from './components/ChatPage';
import RegisterPage from './components/RegisterPage';

function App() {

  return (
    <div className="App">
        <BrowserRouter>
          <Routes>
            <Route path='login' element={<Login/>}></Route>
            <Route path='register' element={<RegisterPage/>}></Route>
            <Route path='*' element={<Navigate to='login' replace></Navigate>}></Route>
            <Route element={<PrivateRoute/>}>
              <Route path='/chats' element={<ChatPage/>}></Route>
            </Route>
          </Routes>
        </BrowserRouter>
    </div>
  );
}

export default App;
