'use client';
import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { isLoggedIn } from '@/lib/auth';
/** * Racine du site : simple aiguillage. 
 * * Connecte -> tableau de bord ; sinon -> connexion.
 *  */
export default function HomePage() {
   const router = useRouter();
 useEffect(() => {
    router.replace(isLoggedIn() ? '/dashboard' : '/login'); }, [router]);
 return null; // rien a afficher, on redirige immediatement
 }

