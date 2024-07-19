/*
 * @Author: lsy
 * @Date: 2024-06-17 14:45:52
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 14:46:08
 * @FilePath: \react-next-app\src\app\dashboard\layout.tsx
 */

// import dynamic from 'next/dynamic';
// const SideNav = dynamic(() => import('@/app/ui/dashboard/sidenav'), { ssr: false });
import SideNav from '@/app/ui/dashboard/sidenav';
export default function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex h-screen flex-col md:flex-row md:overflow-hidden">
      <div className="w-full flex-none md:w-64">
        <SideNav />
      </div>
      <div className="flex-grow p-6 md:overflow-y-auto md:p-12">{children}</div>
    </div>
  );
}