"use client"
/*
 * @Author: lsy
 * @Date: 2024-06-17 14:45:52
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 14:46:08
 * @FilePath: \react-next-app\src\app\dashboard\layout.tsx
 */
import { useRouter } from 'next/navigation'
import { usePathname } from 'next/navigation' 
import React, { useEffect, useState, useMemo } from 'react';
import {
  UserOutlined,
  HomeOutlined,
  BellFilled
} from '@ant-design/icons';
import { Space, notification } from 'antd';
const preFix = 'http://localhost:5000'
const Context = React.createContext({
  name: 'Default',
});
export default function Layout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname()
  const [api, contextHolder] = notification.useNotification();
  const isDetail = pathname.includes('detail')
  const [isUpdate, setIsUpdate] = useState(false)
  const [pushInfoList, setPushInfoList] = useState([])
  useEffect(() => {  
    // 设置定时器  
    const timerId = setInterval(() => {  
      pushInfo() 
    }, 60000);
    // 返回一个清理函数  
    return () => {  
      // 清除定时器  
      clearInterval(timerId);
    };  
  }, []);
  const pushInfo = async () => {
    try {
      const response = await fetch(`${preFix}/api/FollowedCase/SyncedCase/Jingping Gao`);  
      const json = await response.json(); 
      if(!json.isSuccess) throw new Error('');
      const data = json.data
      if(data.length > 0) {
        setPushInfoList(data)
        setIsUpdate(true)
        return data
      } else {
        setIsUpdate(false)
        return []
      }
    } catch (error) {  
      console.error('Error fetching data:', error);  
    }  
  }
  const openNotification = async () => {
    const data = await pushInfo()
    const list = data
    const num = list.length
    api.info({
      message: num > 0 ? `Update Data` : 'No data updates available',
      description: num > 0 ? (
          <ul>
            {
              list.map((item:any) => {
                return <li>{item.caseID}</li>
              })
            }
          </ul>
        ):( <span>No data updates available</span> ),
      placement: 'topRight',
    });
  };
  const goList = () => {
    router.push(`/follow/list`, { scroll: false })
  }
  const contextValue = useMemo(
    () => ({
      name: 'Ant Design',
    }),
    [],
  );
  return (
    <Context.Provider value={contextValue}>
      {contextHolder}
      <Space>
        <div className="flex w-screen h-screen flex-col flex-row md:overflow-hidden">
          <div className="flex w-screen justify-between items-center bg-blue-700 text-[#ffffff] h-[48px] pr-2.5 pl-2.5">
            <div className="flex justify-between cursor-pointer">
              {isDetail &&<HomeOutlined className='pr-5' onClick={goList}/>}
              {isDetail && <p onClick={goList}>Back to My Follow</p>}
            </div>
            <div>{isDetail? 'Follow Detail' : 'Follow List'}</div>
            <div className="flex justify-between">
              <BellFilled className='mr-5 cursor-pointer' onClick={openNotification} style={{color: isUpdate? "#eb2f96" : ""}}/>
              <UserOutlined/>
              <p>User</p>
            </div>
          </div>
          <div style={{height: 'calc(100vh - 48px)'}}>{children}</div>
        </div>
      </Space>
    </Context.Provider>
  );
}