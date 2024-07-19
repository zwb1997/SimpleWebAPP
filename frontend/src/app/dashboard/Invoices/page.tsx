/*
 * @Author: lsy
 * @Date: 2024-06-17 15:06:32
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 17:07:36
 * @FilePath: \react-next-app\src\app\dashboard\Invoices\page.tsx
 */
import { Suspense } from 'react';
import { fetchRevenue } from '@/app/lib/data';
export default async function Page() {
  const revenue = await fetchRevenue() // delete this line
  return (
    <Suspense fallback={<div>Loading...</div>}>
      12332131231
    </Suspense>
  );
}