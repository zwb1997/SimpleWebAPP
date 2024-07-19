/*
 * @Author: lsy
 * @Date: 2024-06-17 14:22:34
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 16:21:20
 * @FilePath: \react-next-app\src\app\layout.tsx
 */
import { AntdRegistry } from '@ant-design/nextjs-registry';
import { inter } from '@/app/ui/fonts';
import type { Metadata } from "next";
import "./globals.css";
export const metadata: Metadata = {
  title: "Create Next App",
  description: "Generated by create next app",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={`${inter.className} antialiased`}>
        <AntdRegistry>{children}</AntdRegistry>  
      </body>
    </html>
  );
}