"use client"
/*
 * @Author: lsy
 * @Date: 2024-06-17 15:06:32
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 17:07:36
 * @FilePath: \react-next-app\src\app\dashboard\Invoices\page.tsx
 */
import { useRouter } from 'next/navigation'
import { Suspense, useState, useEffect} from 'react';
import { Table, Input, Row, Col, Tag, Button, Select, Tooltip, message } from "antd";
import './page.css'
const { Search } = Input;
const preFix = 'http://localhost:5000'
export default function Page() {
  const router = useRouter()
  const [messageApi, contextHolder] = message.useMessage();
  const [listData, setListData] = useState([]);  
  const [pagedInfo, setPagedInfo] = useState({
    current: 1,
    pageSize: 10
  })
  const [inputSearchValue, setInputSearchValue] = useState('');
  const [inputFollowValue, setInputFollowValue] = useState('');
  const [statusType, setStatusType] = useState(undefined)
  const [listLoading, setListLoading] = useState(false)
  const [followLoading, setFollowLoading] = useState(false)
  useEffect(() => {
    fetchData();  
  }, []);
  const fetchData = async () => {
    setListLoading(true) 
    try {
      const params = new URLSearchParams();  
      params.append('PageNumber', '1');  
      params.append('RowsPerPage', '5'); 
      params.append('CaseID', inputSearchValue)
      if(statusType !== undefined){
        params.append('IsArchive', statusType)
      }
      const response = await fetch(`${preFix}/api/FollowedCase/SearchCases?${params.toString()}`);  
      const json = await response.json(); 
      if(!json.isSuccess) throw new Error('');
      const pagedInfo = json.data.pagedInfo
      const pagination = {
        current: pagedInfo.pageNumber,
        pageSize: pagedInfo.pageSize,
        total: pagedInfo.totalRecords
      }

      setPagedInfo(pagination)
      setListData(json?.data?.data || []);  

    } catch (error) {  
      console.error('Error fetching data:', error);  
    }  
    setListLoading(false)
  };
  const handlerUpdateData = async (record:any) => {
    setListLoading(true) 
    try {
      const data = {
        dataId: record.dataId,
        caseID: record.caseID,
        isArchive: !record.isArchive,
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
    setListLoading(false)
  }  
  const handlerFollowCase = async () => {
    setFollowLoading(true) 
    try {
      const response = await fetch(`${preFix}/api/FollowedCase/FollowCase/${inputFollowValue}/Jingping Gao`);  
      const json = await response.json(); 
      if(!json.isSuccess) throw new Error(json.errors);
      messageApi.open({
        type: 'success',
        content: 'success',
      });
      fetchData()
    } catch (error:any) { 
      messageApi.open({
        type: 'error',
        content: error.message || 'Network error',
      });
      console.error('Error handlerFollowCase data:', error);  
    }  
    setFollowLoading(false)
  };
  const goDetail = (id: string) => {
    router.push(`/follow/detail?id=${id}`, { scroll: false })
  }
  const statusTypeOnChange = (e:any) => {
    setStatusType(e)
  }
  const handlerInputSearchChange = (e: any) => {
    setInputSearchValue(e.target.value); 
  }
  const handlerInputFollowChange = (e: any) => {
    setInputFollowValue(e.target.value); 
  }
  const handlerArchive = (record: any) => [
    handlerUpdateData(record)
  ]
  const dataSource = listData
  const columns: any= [
    {
      title: 'Case Subject',
      dataIndex: 'caseSubject',
      key: 'caseSubject',
      ellipsis: {
        showTitle: false,
      },
      render: (text: string) => (
        <Tooltip placement="topLeft" title={text}>
          {text}
        </Tooltip>
      ),
    },
    {
      title: 'Remark',
      dataIndex: 'remark',
      key: 'remark',
      ellipsis: {
        showTitle: false,
      },
      render: (text: string) => (
        <Tooltip placement="topLeft" title={text}>
          {text}
        </Tooltip>
      ),
    },
    {
      title: 'Case ID',
      dataIndex: 'caseID',
      key: 'caseID',
    },
    {
      title: 'Case Severity',
      dataIndex: 'caseSev',
      key: 'caseSev',
      render: (val: string) =>(
        <Tag color="#f50">{val}</Tag>
      )
    },
    {
      title: 'Case Status',
      dataIndex: 'caseStatus',
      key: 'caseStatus',
    },
    {
      title: 'Current Owner',
      dataIndex: 'currentCaseOwner',
      key: 'currentCaseOwner',
    },
    {
      title: 'When followed',
      dataIndex: 'followedTime',
      key: 'followedTime',
    },
    {
      title: 'Action',
      dataIndex: '',
      key: 'Action',
      fixed: 'right',
      width: 250,
      render: (val:string, record:any) => (
        <div className="action">
          <span>View in DFM</span>
          <span onClick={() => goDetail(record.dataId)}>Detail</span>
          <span onClick={() => handlerArchive(record)}>{record.isArchive? `unArchive` : `Archive`}</span>
        </div>
        
      ),
    },
  ];
  const rowSelection = {
    onChange: (selectedRowKeys: any, selectedRows: any) => {
      console.log(`selectedRowKeys: ${selectedRowKeys}`, 'selectedRows: ', selectedRows);
    },
    onSelect: (record: any, selected: any, selectedRows: any) => {
      console.log(record, selected, selectedRows);
    },
    onSelectAll: (selected: any, selectedRows: any, changeRows: any) => {
      console.log(selected, selectedRows, changeRows);
    },
  };
  return (
    <Suspense fallback={<div>Loading...</div>}>
      {contextHolder}
      <div className='mt-5 pr-5 pl-5'>
        <Row justify={'space-between'}>
          <Col span={24} >
            <Search 
              size={'middle'} 
              value={inputFollowValue} 
              onChange={handlerInputFollowChange} 
              placeholder="Please enter Case ID" 
              enterButton="follow" 
              loading={followLoading} 
              onSearch={handlerFollowCase}></Search>
          </Col>
          
        </Row>
        <Row className='mt-5 mb-5' justify="space-between">
          <Col span={8}>
            <div className='flex items-center'>
              <span>Status: </span>
              <Select
                allowClear
                style={{
                  width: '100%'
                  
                }}
                showSearch
                placeholder="Select a Status"
                optionFilterProp="children"
                onChange={statusTypeOnChange}
                options={[
                  {
                    value: true,
                    label: 'Archive',
                  },
                  {
                    value: false,
                    label: 'unArchive',
                  }
                ]}
              />
            </div>
          </Col>
          <Col span={8}>
            <div className='flex items-center'>
              <span className='whitespace-nowrap'>Case ID: </span>
              <Input placeholder="Please enter Case ID" value={inputSearchValue} onChange={handlerInputSearchChange}/>
            </div>
            
          </Col>
          <Col span={4} style={{ textAlign: 'right' }}>
            <Button type="primary" onClick={fetchData}>Search</Button>
          </Col>
        </Row>
        <div className='shadow-lg'>
          <Table 
            rowKey={'dataId'}
            loading={listLoading}
            dataSource={dataSource} 
            columns={columns} 
            pagination={pagedInfo}
            rowSelection={{
              ...rowSelection,
            }}
            scroll={{
              x: 1500,
            }}
          />
        </div>
        
      </div>
      
    </Suspense>
  );
}