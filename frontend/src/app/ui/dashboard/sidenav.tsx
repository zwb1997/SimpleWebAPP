'use client'
/*
 * @Author: lsy
 * @Date: 2024-06-17 15:09:29
 * @LastEditors: lsy
 * @LastEditTime: 2024-06-17 16:49:22
 * @FilePath: \react-next-app\src\app\ui\dashboard\sidenav.tsx
 */
import { AppstoreOutlined, MailOutlined, SettingOutlined } from '@ant-design/icons';
import type { MenuProps } from 'antd';
import { Menu } from 'antd';
import React from 'react';

type MenuItem = Required<MenuProps>['items'][number];

const items: MenuItem[] = [
  {
    key: 'Exchange Online',
    label: 'Exchange Online',
    icon: <MailOutlined />,
    children: [
      { key: 'Mail fLow', label: 'Mail fLow' },
      
      {
        key: 'sub1-menu1',
        label: 'Mailbox',
        children: [
          { key: 'mailbox is full', label: 'mailbox is full' },
        ],
      },
      { key: 'Authentication', label: 'Authentication' },
      { key: 'Graph API', label: 'Graph API' },
    ],
  },
  {
    key: 'Outlook-main',
    label: 'Outlook',
    icon: <AppstoreOutlined />,
    children: [
      { key: 'Outlook', label: 'Outlook' },
      { key: 'New Outlook', label: 'New Outlook' },
      { key: 'Outlook Moblie', label: 'Outlook Moblie' },
      { key: 'OWA', label: 'OWA' },
    ],
  },
  {
    key: 'Diagncstic Tool',
    label: 'Diagncstic Tool',
    icon: <SettingOutlined />,
    children: [],
  },
  // {
  //   key: 'grp',
  //   label: 'Group',
  //   type: 'group',
  //   children: [
  //     { key: '13', label: 'Option 13' },
  //     { key: '14', label: 'Option 14' },
  //   ],
  // },
];

const App: React.FC = () => {
  const onClick: MenuProps['onClick'] = (e) => {
    console.log('click ', e);
  };

  return (
    <Menu
      onClick={onClick}
      style={{ width: 256 }}
      defaultSelectedKeys={['Mail fLow']}
      defaultOpenKeys={['Exchange Online']}
      mode="inline"
      items={items}
    />
  );
};

export default App;