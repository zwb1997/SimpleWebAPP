"use client"
/*
 * @Author: lsy
 * @Date: 2024-06-17 15:06:32
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 17:07:36
 * @FilePath: \react-next-app\src\app\dashboard\Invoices\page.tsx
 */
import { useRouter } from 'next/navigation'; 
import { Suspense, useState, useEffect } from 'react';
import './page.css'
import { Input, Button, Skeleton, message } from 'antd';
const { TextArea } = Input;
const preFix = 'http://localhost:5000'
export default function Page(params:any) {
  
  const [messageApi, contextHolder] = message.useMessage();
  const { id } = params.searchParams
  const [remark, setRemark] = useState('')
  const [resolution, setResolution] = useState('')
  const [detailData, setDetailData] = useState({
    caseSubject: '',
    remark: '',
    resolution: '',
    caseID: '',
    currentCaseOwner: '' ,
    caseSev: '',
    dataId: ''
  })
  const [detailLoading, setDetailLoading] = useState(false)
  const [saveLoading, setSaveLoading] = useState(false)
  useEffect(() => {
    fetchData();  
  }, []);
  const fetchData = async () => {
    setDetailLoading(true) 
    try {
      const response = await fetch(`${preFix}/api/FollowedCase/FollowedCase/${id}`);  
      const json = await response.json(); 
      if(!json.isSuccess) throw new Error('');
      setDetailData(json?.data || {});  
      setRemark(json?.data?.remark || '')
      setResolution(json?.data?.resolution || '')
    } catch (error) {  
      console.error('Error fetching data:', error);  
    }  
    setDetailLoading(false)
  };
  const handlerUpdateData = async (record:any) => {
    setSaveLoading(true) 
    try {
      const data = {
        dataId: detailData.dataId,
        caseID: detailData.caseID,
        remark: remark,
        resolution: resolution,
        whoFollowed: "Jingping Gao"
      }
      const response = await fetch(`${preFix}/api/FollowedCase/FollowCase`,{  
        method: 'POST',  
        headers: {  
          'Content-Type': 'application/json',  
        },  
        body: JSON.stringify(data),  
      });  
      const json = await response.json(); 
      if(!json.isSuccess) throw new Error(''); 
      messageApi.open({
        type: 'success',
        content: 'success',
      });
      fetchData()
    } catch (error) {  
      console.error('Error fetching data:', error);  
    }  
    setSaveLoading(false)
  }  
  return (
    <Suspense fallback={<div>Loading...</div>}>
      {contextHolder}
      <Skeleton active loading={detailLoading}>
        <div className="flex flex-col md:flex-row md:overflow-hidden">
          <div className="flex-grow p-6 md:overflow-y-auto md:p-12">
            <div>
              Case Subject: {detailData.caseSubject}
            </div>
            <div className='mt-5'>
              <div className='flex'>
                <span className='w-20'>remark</span>
                <TextArea
                  value={remark}
                  onChange={(e) => setRemark(e.target.value)}
                  placeholder="Please enter remark"
                  autoSize={{
                    minRows: 3,
                    maxRows: 5,
                  }}
                />
              </div>
              <div className='flex justify-end mt-5'>
                <Button loading={saveLoading} onClick={handlerUpdateData}>save</Button>
              </div>
            </div>
            <div className='mt-5'>
              <div className='flex'>
                <span className='w-20'>Resolution Summary</span>
                <TextArea
                  value={resolution}
                  onChange={(e) => setResolution(e.target.value)}
                  placeholder="Please enter resolution summart"
                  autoSize={{
                    minRows: 3,
                    maxRows: 5,
                  }}
                />
              </div>
              <div className='flex justify-end mt-5'>
                <Button loading={saveLoading} onClick={handlerUpdateData}>save</Button>
              </div>
              
            </div>
          </div>
          <div className="w-full flex-none md:w-80 right-nection mt-10">
            <p>
              <span>Case ID</span>
              <span>{detailData.caseID}</span>
            </p>
            <p>
              <span>Current Owner </span>
              <span>Case Severity</span>
            </p>
            <p>
              <span>{detailData.currentCaseOwner}</span>
              <span>{detailData.caseSev}</span>
            </p>
            <div className='flex justify-between pr-5 mt-5'>
              <Button>View in DFM</Button>
              {/* <Button>Archive this follow</Button> */}
            </div>
          </div>
        </div>
      </Skeleton>
    </Suspense>
  );
}