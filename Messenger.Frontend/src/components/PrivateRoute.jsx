import React from 'react'
import { Outlet, Navigate } from 'react-router-dom';
import {jwtDecode} from 'jwt-decode'

const isTokenValid = (token) => {
    try {
        const decoded = jwtDecode(token);
        return decoded.exp * 1000 > Date.now();
    } catch (e) {
        return false;
    }
}

const PrivateRoute = () => {
    const token = localStorage.getItem('token');
    const isAuthenticated = token && isTokenValid(token);
    return isAuthenticated ? <Outlet/> : <Navigate to='/login'/>
}

export default PrivateRoute;