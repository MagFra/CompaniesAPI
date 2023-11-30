﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Companies.API.Data;
using Companies.API.Entities;
using Companies.API.Dtos.CompaniesDtos;
using AutoMapper;
using Companies.API.Repositorys;

namespace Companies.API.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CompaniesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        // GET: api/Companies
        [HttpGet(Name = "RouteName")]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompany(bool includeEmployees = false)
        {
            //Dont work!!!
            //var dtos = includeEmployees ? mapper.ProjectTo<CompanyDto>(_context.Companies.Include(c => c.Employees)) :
            //                              mapper.ProjectTo<CompanyDto>(_context.Companies); 

            var dtos = includeEmployees ? mapper.Map<IEnumerable<CompanyDto>>(await unitOfWork.CompanyRepository.GetAsync(includeEmployees: true)) :
                                          mapper.Map<IEnumerable<CompanyDto>>(await unitOfWork.CompanyRepository.GetAsync());

            return Ok(dtos);
        }

       

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(Guid id)
        {
            Company? company = await unitOfWork.CompanyRepository.GetAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            var companyDto = mapper.Map<CompanyDto>(company);

            return Ok(companyDto);
        }


        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(Guid id, CompanyForUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var existingCompany = await unitOfWork.CompanyRepository.GetAsync(id);

            if (existingCompany == null) return NotFound();

            mapper.Map(dto, existingCompany);
            await unitOfWork.CompleteAsync();

           // return NoContent();
           return Ok(mapper.Map<CompanyDto>(existingCompany)); //Only for demo!!!! 
        }

        //// POST: api/Companies
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(CompanyForCreationDto dto)
        {
            var company = mapper.Map<Company>(dto);

            await unitOfWork.CompanyRepository.AddAsync(company);
            // _context.Companies.Add(company);
            await unitOfWork.CompleteAsync();

            var companyToReturn = mapper.Map<CompanyDto>(company);

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, companyToReturn);
        }

        //// DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = await unitOfWork.CompanyRepository.GetAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            unitOfWork.CompanyRepository.Remove(company);
            // _context.Companies.Remove(company);
            await unitOfWork.CompleteAsync();

            return NoContent();
        }

        //private bool CompanyExists(Guid id)
        //{
        //    return _context.Companies.Any(e => e.Id == id);
        //}
    }
}
