import React, { useState } from 'react'
import { getAllCompanyWorkers, WorkerDto } from '../api/accountApi'

type Props = {}

const ManagerPanel = (props: Props) => {
    const[companyWorkers, setCompanyWorkers] = useState<WorkerDto[]>([]);

    const getAllWorkers = async ()=>{
        try{
            const workers = await getAllCompanyWorkers();
            setCompanyWorkers(workers);
        }catch(err:any){
            console.log(err, "Unable to find workers in you company");
        }
    }


  return (
    <div>
        <button onClick={getAllWorkers}>get workers</button>
        {companyWorkers && companyWorkers.length > 0 && (
            <ul>
                {companyWorkers.map(worker =>(
                    <li key ={worker.id}>
                        <p>{worker.name}</p>
                        <p>{worker.email}</p>
                        <p>{worker.role}</p>
                        <p>{worker.companyMembershipStatus}</p>
                    </li>
                ))}
            </ul>
        )}
    </div>
  )
}
export default ManagerPanel;